using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using NetworkSocket.Fast.Context;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp服务端抽象类 
    /// </summary>
    public abstract class FastTcpServerBase : TcpServerBase<FastPacket, FastPacket>, IFastTcpServer
    {
        /// <summary>
        /// 所有服务行为
        /// </summary>
        private FastActionList fastActionList;

        /// <summary>
        /// 数据包哈希码提供者
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
        /// 获取或设置服务行为特性过滤器提供者
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 快速构建Tcp服务端
        /// </summary>
        public FastTcpServerBase()
        {
            this.fastActionList = new FastActionList();
            this.packetIdProvider = new PacketIdProvider();
            this.taskSetActionTable = new TaskSetActionTable();

            this.Serializer = new DefaultSerializer();
            this.FilterAttributeProvider = new FilterAttributeProvider();
        }

        /// <summary>
        /// 绑定本程序集所有实现IFastService的服务
        /// </summary>
        /// <returns></returns>       
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService()
        {
            var allServices = this.GetType().Assembly.GetTypes().Where(item => typeof(IFastService).IsAssignableFrom(item));
            return this.BindService(allServices);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>       
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService<T>() where T : IFastService
        {
            return this.BindService(typeof(T));
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService(params Type[] serviceType)
        {
            return this.BindService((IEnumerable<Type>)serviceType);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="serivceType">服务类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService(IEnumerable<Type> serivceType)
        {
            if (serivceType == null)
            {
                throw new ArgumentNullException("serivceType");
            }

            if (serivceType.Any(item => item == null))
            {
                throw new ArgumentException("serivceType不能含null值");
            }

            if (serivceType.Any(item => typeof(IFastService).IsAssignableFrom(item) == false))
            {
                throw new ArgumentException("serivceType必须派生于IFastService");
            }

            foreach (var type in serivceType)
            {
                var actions = FastTcpCommon.GetServiceFastActions(type);
                this.fastActionList.AddRange(actions);
            }
            return this;
        }


        /// <summary>
        /// 调用客户端实现的Api      
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param>    
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception>         
        public Task InvokeApi(IClient<FastPacket> client, string api, params object[] parameters)
        {
            return Task.Factory.StartNew(() =>
            {
                var id = this.packetIdProvider.GetId();
                var packet = new FastPacket(api, id, false);
                packet.SetBodyParameters(this.Serializer, parameters);
                client.Send(packet);
            });
        }

        /// <summary>
        /// 调用客户端实现的Api    
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="api">Api</param>
        /// <param name="parameters">参数</param>     
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception> 
        /// <exception cref="RemoteException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <returns>远程数据任务</returns>  
        public Task<T> InvokeApi<T>(IClient<FastPacket> client, string api, params object[] parameters)
        {
            var id = this.packetIdProvider.GetId();
            return FastTcpCommon.InvokeApi<T>(client, this.taskSetActionTable, this.Serializer, api, id, false, parameters);
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法        
        /// 返回的每一个数据包，将触发一次OnRecvComplete方法       
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="builder">接收到的历史数据</param>
        /// <returns></returns>
        protected override IEnumerable<FastPacket> OnReceive(IClient<FastPacket> client, ByteBuilder builder)
        {
            FastPacket packet;
            while ((packet = FastPacket.From(builder)) != null)
            {
                yield return packet;
            }
        }

        /// <summary>
        /// 当接收到客户端数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="tRecv">接收到的数据类型</param>
        protected override void OnRecvComplete(IClient<FastPacket> client, FastPacket tRecv)
        {
            var requestContext = new ServerRequestContext { Client = client, Packet = tRecv, FastTcpServer = this };

            if (tRecv.IsException)
            {
                this.ProcessRemoteException(requestContext);
            }
            else
            {
                this.ProcessRequest(requestContext);
            }
        }

        /// <summary>
        /// 处理远返回的程异常
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        private void ProcessRemoteException(ServerRequestContext requestContext)
        {
            var remoteException = FastTcpCommon.SetFastActionTaskException(this.Serializer, this.taskSetActionTable, requestContext);
            if (remoteException == null)
            {
                return;
            }
            var exceptionContext = new ServerExceptionContext(requestContext, remoteException);
            this.ExecExceptionFilters(exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exceptionContext.Exception;
            }
        }

        /// <summary>
        /// 处理正常的数据请求
        /// </summary>
        /// <param name="requestContext">请求上下文</param>       
        private void ProcessRequest(ServerRequestContext requestContext)
        {
            if (requestContext.Packet.IsFromClient == false)
            {
                FastTcpCommon.SetFastActionTaskResult(requestContext, this.taskSetActionTable);
                return;
            }

            var action = this.GetFastAction(requestContext);
            if (action == null)
            {
                return;
            }

            var actionContext = new ServerActionContext(requestContext, action);
            var fastService = this.GetFastService(actionContext);
            if (fastService == null)
            {
                return;
            }

            // 执行服务行为           
            fastService.Execute(actionContext);

            // 释放资源
            DependencyResolver.Current.TerminateService(fastService);
        }

        /// <summary>
        /// 获取服务行为
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        private FastAction GetFastAction(ServerRequestContext requestContext)
        {
            var action = this.fastActionList.TryGet(requestContext.Packet.ApiName);
            if (action != null)
            {
                return action;
            }

            var exception = new ApiNotExistException(requestContext.Packet.ApiName);
            var exceptionContext = new ServerExceptionContext(requestContext, exception);

            FastTcpCommon.SetRemoteException(this.Serializer, exceptionContext);
            this.ExecExceptionFilters(exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exception;
            }

            return null;
        }

        /// <summary>
        /// 获取FastService实例
        /// </summary>
        /// <param name="actionContext">服务行为上下文</param>
        /// <returns></returns>
        private IFastService GetFastService(ServerActionContext actionContext)
        {
            // 获取服务实例
            var fastService = (IFastService)DependencyResolver.Current.GetService(actionContext.Action.DeclaringService);
            if (fastService != null)
            {
                return fastService;
            }
            var exception = new Exception(string.Format("无法获取类型{0}的实例", actionContext.Action.DeclaringService));
            var exceptionContext = new ServerExceptionContext(actionContext, exception);

            FastTcpCommon.SetRemoteException(this.Serializer, exceptionContext);
            this.ExecExceptionFilters(exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exception;
            }

            return null;
        }


        /// <summary>
        /// 执行异常过滤器
        /// </summary>         
        /// <param name="exceptionContext">上下文</param>       
        private void ExecExceptionFilters(ServerExceptionContext exceptionContext)
        {
            foreach (var filter in GlobalFilters.ExceptionFilters)
            {
                if (exceptionContext.ExceptionHandled == false)
                {
                    filter.OnException(exceptionContext);
                }
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
            if (disposing)
            {
                this.fastActionList = null;

                this.taskSetActionTable.Clear();
                this.taskSetActionTable = null;

                this.packetIdProvider = null;
                this.Serializer = null;
                this.FilterAttributeProvider = null;
            }
        }
        #endregion
    }
}
