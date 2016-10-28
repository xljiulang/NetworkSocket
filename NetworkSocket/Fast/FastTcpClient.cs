using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示Fast协议的tcp客户端
    /// </summary>
    public class FastTcpClient : TcpClientBase
    {
        /// <summary>
        /// 所有Api行为
        /// </summary>
        private ApiActionList apiActionList;

        /// <summary>
        /// 数据包id提供者
        /// </summary>
        private PacketIdProvider packetIdProvider;

        /// <summary>
        /// 任务行为表
        /// </summary>
        private TaskSetterTable<long> taskSetterTable;


        /// <summary>
        /// 获取或设置序列化工具
        /// 默认是Json序列化
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 获取或设置请求等待超时时间(毫秒) 
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TimeSpan TimeOut { get; set; }

        /// <summary>
        /// Fast协议的tcp客户端
        /// </summary>
        public FastTcpClient()
        {
            this.TimeOut = TimeSpan.FromSeconds(30);
            this.Init();
        }

        /// <summary>
        /// SSL支持的Fast协议的tcp客户端
        /// </summary>
        /// <param name="targetHost">目标主机</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FastTcpClient(string targetHost)
            : base(targetHost)
        {
            this.Init();
        }

        /// <summary>
        /// SSL支持的Fast协议的tcp客户端
        /// </summary>  
        /// <param name="targetHost">目标主机</param>
        /// <param name="certificateValidationCallback">远程证书验证回调</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FastTcpClient(string targetHost, RemoteCertificateValidationCallback certificateValidationCallback)
            : base(targetHost, certificateValidationCallback)
        {
            this.Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            this.apiActionList = new ApiActionList(Common.GetServiceApiActions(this.GetType()));
            this.packetIdProvider = new PacketIdProvider();
            this.taskSetterTable = new TaskSetterTable<long>();
            this.Serializer = new DefaultSerializer();
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法
        /// </summary>
        /// <param name="inputStream">接收到的历史数据</param>        
        protected sealed override void OnReceive(IStreamReader inputStream)
        {
            var packages = this.GenerateFastPackets(inputStream);
            ThreadPool.UnsafeQueueUserWorkItem((state) =>
            {
                foreach (var package in packages)
                {
                    this.OnReceivePacket(package);
                }
            }, null);
        }

        /// <summary>
        /// 生成数据包
        /// </summary>
        /// <param name="inputStream">数据流</param>
        /// <returns></returns>
        private IList<FastPacket> GenerateFastPackets(IStreamReader inputStream)
        {
            var list = new List<FastPacket>();
            while (true)
            {
                var packet = default(FastPacket);
                if (FastPacket.Parse(inputStream, out packet) == false)
                {
                    return list;
                }
                if (packet == null)
                {
                    return list;
                }
                list.Add(packet);
            }
        }


        /// <summary>
        /// 接收到服务发来的数据包
        /// </summary>
        /// <param name="packet">数据包</param>
        private async void OnReceivePacket(FastPacket packet)
        {
            var requestContext = new RequestContext(null, packet, null);
            if (packet.IsException == true)
            {
                Common.SetApiActionTaskException(this.taskSetterTable, requestContext);
            }
            else
            {
                await this.ProcessRequestAsync(requestContext);
            }
        }


        /// <summary>
        /// 处理正常的数据请求
        /// </summary>      
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        private async Task ProcessRequestAsync(RequestContext requestContext)
        {
            if (requestContext.Packet.IsFromClient)
            {
                Common.SetApiActionTaskResult(requestContext, this.taskSetterTable, this.Serializer);
            }
            else
            {
                var action = this.GetApiAction(requestContext);
                if (action != null)
                {
                    var actionContext = new ActionContext(requestContext, action);
                    await this.ExecuteActionAsync(actionContext);
                }
            }
        }

        /// <summary>
        /// 获取Api行为
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        private ApiAction GetApiAction(RequestContext requestContext)
        {
            var action = this.apiActionList.TryGet(requestContext.Packet.ApiName);
            if (action != null)
            {
                return action;
            }

            var exception = new ApiNotExistException(requestContext.Packet.ApiName);
            var exceptionContext = new ExceptionContext(requestContext, exception);
            Common.SendRemoteException(this.UnWrap(), exceptionContext);

            var exceptionHandled = false;
            this.OnException(requestContext.Packet, exception, out exceptionHandled);
            if (exceptionHandled == false)
            {
                throw exception;
            }

            return null;
        }         


        /// <summary>
        /// 执行Api行为
        /// </summary>
        /// <param name="actionContext">上下文</param>   
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        private async Task ExecuteActionAsync(ActionContext actionContext)
        {
            try
            {
                var parameters = Common.GetAndUpdateParameterValues(this.Serializer, actionContext);
                var result = await actionContext.Action.ExecuteAsync(this, actionContext.Action.ParameterValues);
                if (actionContext.Action.IsVoidReturn == false && this.IsConnected)
                {
                    actionContext.Packet.Body = this.Serializer.Serialize(result);
                    this.Send(actionContext.Packet.ToByteRange());
                }
            }
            catch (Exception ex)
            {
                this.ProcessExecutingException(actionContext, ex);
            }
        }


        /// <summary>
        /// 处理Api行为执行过程中产生的异常
        /// </summary>
        /// <param name="actionContext">上下文</param>       
        /// <param name="exception">异常项</param>
        private void ProcessExecutingException(ActionContext actionContext, Exception exception)
        {
            var exceptionContext = new ExceptionContext(actionContext, new ApiExecuteException(exception));
            Common.SendRemoteException(this.UnWrap(), exceptionContext);

            var exceptionHandled = false;
            this.OnException(actionContext.Packet, exception, out exceptionHandled);
            if (exceptionHandled == false)
            {
                throw exception;
            }
        }

        /// <summary>
        ///  当操作中遇到处理异常时，将触发此方法
        /// </summary>
        /// <param name="packet">数据包对象</param>
        /// <param name="exception">异常对象</param>
        /// <param name="exceptionHandled">异常是否已处理</param>
        protected virtual void OnException(FastPacket packet, Exception exception, out bool exceptionHandled)
        {
            exceptionHandled = false;
        }

        /// <summary>
        /// 调用服务端实现的Api        
        /// </summary>       
        /// <param name="api">Api行为的api</param>
        /// <param name="parameters">参数列表</param>          
        /// <exception cref="SocketException"></exception> 
        /// <exception cref="SerializerException"></exception> 
        public void InvokeApi(string api, params object[] parameters)
        {
            var packet = new FastPacket(api, this.packetIdProvider.NewId(), true);
            packet.SetBodyParameters(this.Serializer, parameters);
            this.Send(packet.ToByteRange());
        }

        /// <summary>
        /// 调用服务端实现的Api   
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="api">Api行为的api</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="SocketException"></exception>        
        /// <exception cref="SerializerException"></exception>
        /// <returns>远程数据任务</returns>    
        public Task<T> InvokeApi<T>(string api, params object[] parameters)
        {
            var id = this.packetIdProvider.NewId();
            var packet = new FastPacket(api, id, true);
            packet.SetBodyParameters(this.Serializer, parameters);
            return Common.InvokeApi<T>(this.UnWrap(), this.taskSetterTable, this.Serializer, packet, this.TimeOut);
        }

        /// <summary>
        /// 断开时清除数据任务列表  
        /// </summary>
        protected override void OnDisconnected()
        {
            var taskSetActions = this.taskSetterTable.TakeAll();
            foreach (var taskSetAction in taskSetActions)
            {
                var exception = new SocketException(SocketError.Shutdown.GetHashCode());
                taskSetAction.SetException(exception);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this.apiActionList = null;
            this.taskSetterTable.Clear();
            this.taskSetterTable = null;
            this.packetIdProvider = null;
            this.Serializer = null;
        }
    }
}
