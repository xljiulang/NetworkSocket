using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp客户端
    /// </summary>
    public class FastTcpClient : TcpClientBase, IFastTcpClient
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
        private TaskSetActionTable taskSetActionTable;

        /// <summary>
        /// 获取或设置请求等待超时时间(毫秒) 
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int TimeOut
        {
            get
            {
                return this.taskSetActionTable.TimeOut;
            }
            set
            {
                this.taskSetActionTable.TimeOut = value;
            }
        }

        /// <summary>
        /// 获取或设置序列化工具
        /// 默认是Json序列化
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 快速构建Tcp服务端
        /// </summary>
        public FastTcpClient()
        {
            this.apiActionList = new ApiActionList(FastTcpCommon.GetServiceApiActions(this.GetType()));
            this.packetIdProvider = new PacketIdProvider();
            this.taskSetActionTable = new TaskSetActionTable();
            this.Serializer = new DefaultSerializer();
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法
        /// </summary>
        /// <param name="buffer">接收到的历史数据</param>        
        protected override void OnReceive(ReceiveBuffer buffer)
        {
            while (true)
            {
                var packet = FastPacket.From(buffer);
                if (packet == null)
                {
                    break;
                }
                // 新线程处理业务内容
                Task.Factory.StartNew(() => this.OnRecvPacket(packet));
            }
        }
         
        /// <summary>
        /// 接收到服务发来的数据包
        /// </summary>
        /// <param name="packet">数据包</param>
        private void OnRecvPacket(FastPacket packet)
        {
            var requestContext = new RequestContext(null, packet, null);
            if (packet.IsException == false)
            {
                this.ProcessRequest(requestContext);
            }
            else
            {
                this.ProcessRemoteException(requestContext);
            }
        }

        /// <summary>
        /// 处理远返回的程异常
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        private void ProcessRemoteException(RequestContext requestContext)
        {
            var remoteException = FastTcpCommon.SetApiActionTaskException(this.taskSetActionTable, requestContext);
            if (remoteException == null)
            {
                return;
            }

            var exceptionHandled = false;
            this.OnException(requestContext.Packet, remoteException, out exceptionHandled);

            if (exceptionHandled == false)
            {
                throw remoteException;
            }
        }

        /// <summary>
        /// 处理正常的数据请求
        /// </summary>      
        /// <param name="requestContext">请求上下文</param>
        private void ProcessRequest(RequestContext requestContext)
        {
            if (requestContext.Packet.IsFromClient)
            {
                FastTcpCommon.SetApiActionTaskResult(requestContext, this.taskSetActionTable);
                return;
            }

            var action = this.GetApiAction(requestContext);
            if (action == null)
            {
                return;
            }

            var actionContext = new ActionContext(requestContext, action);
            this.TryExecuteAction(actionContext);
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
            FastTcpCommon.SetRemoteException(this, exceptionContext);

            var exceptionHandled = false;
            this.OnException(requestContext.Packet, exception, out exceptionHandled);
            if (exceptionHandled == false)
            {
                throw exception;
            }

            return null;
        }


        /// <summary>
        /// 调用自身方法
        /// 将返回值发送给服务器
        /// 或将异常发送给服务器
        /// </summary>    
        /// <param name="actionContext">上下文</param>       
        private void TryExecuteAction(ActionContext actionContext)
        {
            try
            {
                this.ExecuteAction(actionContext);
            }
            catch (AggregateException exception)
            {
                foreach (var inner in exception.InnerExceptions)
                {
                    this.ProcessExecutingException(actionContext, inner);
                }
            }
            catch (Exception exception)
            {
                this.ProcessExecutingException(actionContext, exception);
            }
        }


        /// <summary>
        /// 执行Api行为
        /// </summary>
        /// <param name="actionContext">上下文</param>   
        /// <exception cref="SerializerException"></exception>
        private void ExecuteAction(ActionContext actionContext)
        {
            var action = actionContext.Action;
            var packet = actionContext.Packet;

            action.ParameterValues = packet.GetBodyParameters(this.Serializer, action.ParameterTypes);
            var returnValue = action.Execute(this, action.ParameterValues);

            if (action.IsVoidReturn == false && this.IsConnected)
            {
                packet.Body = this.Serializer.Serialize(returnValue);
                this.Send(packet.ToByteRange());
            }
        }

        /// <summary>
        /// 处理Api行为执行过程中产生的异常
        /// </summary>
        /// <param name="actionContext">上下文</param>       
        /// <param name="exception">异常项</param>
        private void ProcessExecutingException(ActionContext actionContext, Exception exception)
        {
            var exceptionContext = new ExceptionContext(actionContext, new ApiExecuteException(actionContext, exception));
            FastTcpCommon.SetRemoteException(this, exceptionContext);

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
        /// <exception cref="ProtocolException"></exception>
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
        /// <exception cref="ProtocolException"></exception>
        /// <returns>远程数据任务</returns>    
        public Task<T> InvokeApi<T>(string api, params object[] parameters)
        {
            var id = this.packetIdProvider.NewId();
            var packet = new FastPacket(api, id, true);
            packet.SetBodyParameters(this.Serializer, parameters);
            return FastTcpCommon.InvokeApi<T>(this, this.taskSetActionTable, this.Serializer, packet);
        }

        /// <summary>
        /// 当与服务器断开连接时，将触发此方法
        /// 并触发未完成的请求产生SocketException异常
        /// </summary>
        protected override void OnDisconnect()
        {
            var taskSetActions = this.taskSetActionTable.TakeAll();
            foreach (var taskSetAction in taskSetActions)
            {
                taskSetAction.SetAction(SetTypes.SetShutdownException, null);
            }
        }

        #region IDisponse
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.taskSetActionTable.Dispose();

            if (disposing)
            {
                this.apiActionList = null;

                this.taskSetActionTable.Clear();
                this.taskSetActionTable = null;

                this.packetIdProvider = null;
                this.Serializer = null;
            }
        }
        #endregion
    }
}
