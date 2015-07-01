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

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp服务端
    /// </summary>
    public class FastTcpServer : TcpServerBase<FastSession>, IFastTcpServer
    {
        /// <summary>
        /// 所有Api行为
        /// </summary>
        private ApiActionList apiActionList;

        /// <summary>
        /// 获取数据包id提供者
        /// </summary>
        internal PacketIdProvider PacketIdProvider { get; private set; }

        /// <summary>
        /// 获取任务行为记录表
        /// </summary>
        internal TaskSetActionTable TaskSetActionTable { get; private set; }

        /// <summary>
        /// 获取或设置请求等待超时时间(毫秒)    
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int TimeOut
        {
            get
            {
                return this.TaskSetActionTable.TimeOut;
            }
            set
            {
                this.TaskSetActionTable.TimeOut = value;
            }
        }

        /// <summary>
        /// 获取或设置序列化工具
        /// 默认是Json序列化
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 获取全局过滤器
        /// </summary>
        public GlobalFilters GlobalFilter { get; private set; }

        /// <summary>
        /// 获取或设置依赖关系解析提供者
        /// </summary>
        public IDependencyResolver DependencyResolver { get; set; }

        /// <summary>
        /// 获取或设置Api行为特性过滤器提供者
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 快速构建Tcp服务端
        /// </summary>
        public FastTcpServer()
        {
            this.apiActionList = new ApiActionList();
            this.PacketIdProvider = new PacketIdProvider();
            this.TaskSetActionTable = new TaskSetActionTable();

            this.Serializer = new DefaultSerializer();
            this.GlobalFilter = new GlobalFilters();
            this.DependencyResolver = new DefaultDependencyResolver();
            this.FilterAttributeProvider = new FilterAttributeProvider();
        }

        /// <summary>
        /// 绑定程序集下所有实现IFastApiService的服务
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public FastTcpServer BindService(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException();
            }

            var apiServices = assembly.GetTypes().Where(item =>
                item.IsAbstract == false
                && item.IsInterface == false
                && typeof(IFastApiService).IsAssignableFrom(item));

            if (apiServices.Count() == 0)
            {
                throw new ArgumentException(string.Format("程序集{0}不包含任何{1}服务", assembly.GetName().Name, typeof(IFastApiService).Name));
            }

            return this.BindService(apiServices);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>        
        /// <exception cref="ArgumentException"></exception>       
        /// <returns></returns>
        public FastTcpServer BindService<T>() where T : IFastApiService
        {
            return this.BindService(typeof(T));
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="apiServiceType">Api服务类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public FastTcpServer BindService(params Type[] apiServiceType)
        {
            return this.BindService((IEnumerable<Type>)apiServiceType);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="apiServiceType">Api服务类型</param>        
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public FastTcpServer BindService(IEnumerable<Type> apiServiceType)
        {
            if (apiServiceType == null)
            {
                throw new ArgumentNullException("apiServiceType");
            }

            if (apiServiceType.Count() == 0)
            {
                throw new ArgumentOutOfRangeException("apiServiceType", "apiServiceType集合不能为空");
            }

            if (apiServiceType.Any(item => item == null))
            {
                throw new ArgumentException("apiServiceType不能含null值");
            }

            if (apiServiceType.Any(item => item.IsInterface || item.IsAbstract))
            {
                throw new ArgumentException("apiServiceType不能是接口或抽象类");
            }

            if (apiServiceType.Any(item => typeof(IFastApiService).IsAssignableFrom(item) == false))
            {
                throw new ArgumentException(string.Format("apiServiceType必须派生于{0}", typeof(IFastApiService).Name));
            }

            foreach (var type in apiServiceType)
            {
                var actions = FastTcpCommon.GetServiceApiActions(type);
                this.apiActionList.AddRange(actions);
            }
            return this;
        }

        /// <summary>
        /// 创建新的会话对象
        /// </summary>
        /// <returns></returns>
        protected override FastSession OnCreateSession()
        {
            return new FastSession(this);
        }

        /// <summary>
        /// 当接收到会话对象的数据时，将触发此方法  
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="buffer">接收到的历史数据</param>
        /// <returns></returns>
        protected override void OnReceive(FastSession session, ReceiveBuffer buffer)
        {
            while (true)
            {
                FastPacket packet = null;
                try
                {
                    packet = FastPacket.From(buffer);
                }
                catch (Exception ex)
                {
                    buffer.Clear();
                    this.OnException(session, ex);
                }

                if (packet == null)
                {
                    break;
                }
                // 新线程处理业务内容
                Task.Factory.StartNew(() => this.OnRecvPacket(session, packet));
            }
        }


        /// <summary>
        /// 接收到会话对象的数据包
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="packet">数据包</param>
        private void OnRecvPacket(FastSession session, FastPacket packet)
        {
            var requestContext = new RequestContext(session, packet, this.AllSessions);

            if (packet.IsException)
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
        private void ProcessRemoteException(RequestContext requestContext)
        {
            var remoteException = FastTcpCommon.SetApiActionTaskException(this.TaskSetActionTable, requestContext);
            if (remoteException != null)
            {
                var exceptionContext = new ExceptionContext(requestContext, remoteException);
                this.ExecGlobalExceptionFilters(exceptionContext);
            }
        }

        /// <summary>
        /// 处理正常的数据请求
        /// </summary>
        /// <param name="requestContext">请求上下文</param>       
        private void ProcessRequest(RequestContext requestContext)
        {
            if (requestContext.Packet.IsFromClient == false)
            {
                FastTcpCommon.SetApiActionTaskResult(requestContext, this.TaskSetActionTable);
                return;
            }

            var action = this.GetApiAction(requestContext);
            if (action != null)
            {
                var actionContext = new ActionContext(requestContext, action);
                var fastApiService = this.GetFastApiService(actionContext);
                if (fastApiService != null)
                {
                    // 执行Api行为           
                    fastApiService.Execute(actionContext);
                    // 释放资源
                    this.DependencyResolver.TerminateService(fastApiService);
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
            if (action == null)
            {
                var exception = new ApiNotExistException(requestContext.Packet.ApiName);
                var exceptionContext = new ExceptionContext(requestContext, exception);

                FastTcpCommon.SetRemoteException(requestContext.Session, exceptionContext);
                this.ExecGlobalExceptionFilters(exceptionContext);
            }
            return action;
        }

        /// <summary>
        /// 获取FastApiService实例
        /// </summary>
        /// <param name="actionContext">Api行为上下文</param>
        /// <returns></returns>
        private IFastApiService GetFastApiService(ActionContext actionContext)
        {
            IFastApiService fastApiService = null;
            Exception innerException = null;

            try
            {
                fastApiService = (IFastApiService)this.DependencyResolver.GetService(actionContext.Action.DeclaringService);
            }
            catch (Exception ex)
            {
                innerException = ex;
            }

            if (fastApiService == null)
            {
                var exception = new ResolveException(actionContext.Action.DeclaringService, innerException);
                var exceptionContext = new ExceptionContext(actionContext, exception);

                FastTcpCommon.SetRemoteException(actionContext.Session, exceptionContext);
                this.ExecGlobalExceptionFilters(exceptionContext);
            }
            return fastApiService;
        }

        /// <summary>
        /// 执行异常过滤器
        /// </summary>         
        /// <param name="exceptionContext">上下文</param>       
        private void ExecGlobalExceptionFilters(ExceptionContext exceptionContext)
        {
            foreach (var filter in this.GlobalFilter.ExceptionFilters)
            {
                if (exceptionContext.ExceptionHandled == false)
                {
                    filter.OnException(exceptionContext);
                }
                else
                {
                    break;
                }
            }

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exceptionContext.Exception;
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
            this.TaskSetActionTable.Dispose();

            if (disposing)
            {
                this.apiActionList = null;

                this.TaskSetActionTable.Clear();
                this.TaskSetActionTable = null;

                this.PacketIdProvider = null;
                this.Serializer = null;
                this.GlobalFilter = null;
                this.DependencyResolver = null;
                this.FilterAttributeProvider = null;
            }
        }
        #endregion
    }
}
