using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp服务端抽象类 
    /// </summary>
    public abstract class FastTcpServerBase : TcpServerBase<FastPacket>, IAuthorizationFilter, IActionFilter
    {
        /// <summary>
        /// 所有服务行为
        /// </summary>
        private List<FastAction> serviceActions;

        /// <summary>
        /// 特殊服务
        /// </summary>
        private SpecialService specialService;

        /// <summary>
        /// 服务实例反转缓存
        /// </summary>
        private ConcurrentDictionary<Type, FastServiceBase> serviceResolver;

        /// <summary>
        /// 获取或设置序列化工具
        /// 默认是Json序列化
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 快速构建Tcp服务端
        /// </summary>
        public FastTcpServerBase()
        {
            this.serviceActions = new List<FastAction>();
            this.specialService = new SpecialService();
            this.serviceResolver = new ConcurrentDictionary<Type, FastServiceBase>();
            this.Serializer = new DefaultSerializer();

            // 添加特殊服务行为
            var specialActions = FastTcpCommon.GetServiceActions(typeof(SpecialService));
            this.serviceActions.AddRange(specialActions);

            // 添加到全局过滤器
            GlobalFilters.Add(this);
        }

        /// <summary>
        /// 绑定本程序集所有的服务
        /// </summary>
        public void BindService()
        {
            var allServices = this.GetType().Assembly.GetTypes().Where(item => typeof(FastServiceBase).IsAssignableFrom(item));
            this.BindService(allServices);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        public void BindService<T>() where T : FastServiceBase
        {
            this.BindService(typeof(T));
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        public void BindService(params Type[] serviceType)
        {
            this.BindService((IEnumerable<Type>)serviceType);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="serivceType">服务类型</param>
        public void BindService(IEnumerable<Type> serivceType)
        {
            if (serivceType == null)
            {
                throw new ArgumentNullException("serivceType");
            }

            if (serivceType.Any(item => item == null))
            {
                throw new ArgumentException("serivceType不能含null值");
            }

            if (serivceType.Any(item => typeof(FastServiceBase).IsAssignableFrom(item) == false))
            {
                throw new ArgumentException("serivceType必须派生于FastServiceBase");
            }

            foreach (var type in serivceType)
            {
                var actions = FastTcpCommon.GetServiceActions(type);
                this.serviceActions.AddRange(actions);
            }

            FastTcpCommon.CheckActionsRepeatCommand(this.serviceActions);
            FastTcpCommon.CheckActionsContract(this.serviceActions);
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法
        /// 此方法用于处理和分析收到的数据
        /// 如果得到一个数据包，将触发OnRecvComplete方法
        /// [注]这里只需处理一个数据包的流程
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="recvBuilder">接收到的历史数据</param>
        /// <returns>如果不够一个数据包，则请返回null</returns>
        protected override FastPacket OnReceive(SocketAsync<FastPacket> client, ByteBuilder recvBuilder)
        {
            return FastPacket.GetPacket(recvBuilder);
        }

        /// <summary>
        /// 当接收到客户端数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        protected override void OnRecvComplete(SocketAsync<FastPacket> client, FastPacket packet)
        {
            if (packet.IsException == false)
            {
                this.ProcessNormalPacket(client, packet);
                return;
            }

            // 抛出远程异常
            var exception = FastTcpCommon.ThrowRemoteException(packet, this.Serializer);
            if (exception != null)
            {
                var exceptionContext = new ExceptionContext { Client = client, Packet = packet, Exception = exception };
                this.OnException(exceptionContext);
            }
        }

        /// <summary>
        /// 处理正常数据包
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        private void ProcessNormalPacket(SocketAsync<FastPacket> client, FastPacket packet)
        {
            var requestContext = new RequestContext { Client = client, Packet = packet };
            var action = this.serviceActions.Find(item => item.Command == packet.Command);

            if (action == null)
            {
                var exception = new Exception(string.Format("命令为{0}的服务行为不存在", packet.Command));
                this.RaiseException(new ExceptionContext(requestContext, exception));
                return;
            }

            var isSpecail = true;
            var fastService = (FastServiceBase)this.specialService;

            if (Enum.IsDefined(typeof(SpecialCommands), packet.Command) == false)
            {
                fastService = this.GetService(action.DeclaringService);
                isSpecail = false;
            }

            if (fastService == null)
            {
                var ex = new Exception(string.Format("无法获取类型{0}的实例", action.DeclaringService));
                this.RaiseException(new ExceptionContext(requestContext, ex));
                return;
            }

            // 处理数据包           
            fastService.Serializer = this.Serializer;
            fastService.GetFilters = this.GetFilters;
            fastService.ProcessAction(new ActionContext(requestContext, action));

            if (isSpecail == false)
            {
                this.DisposeService(fastService);
            }
        }


        /// <summary>
        /// 获取服务实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        protected virtual FastServiceBase GetService(Type serviceType)
        {
            return this.serviceResolver.GetOrAdd(serviceType, type => Activator.CreateInstance(type) as FastServiceBase);
        }

        /// <summary>
        /// 释放服务资源
        /// </summary>
        /// <param name="service">服务实例</param>
        protected virtual void DisposeService(FastServiceBase service)
        {
            service.Dispose();
        }

        /// <summary>
        /// 获取服务行为的过滤器
        /// 不包括全局过滤器
        /// </summary>
        /// <param name="action">服务行为</param>
        /// <returns></returns>
        protected virtual IEnumerable<Filter> GetFilters(FastAction action)
        {
            var actionAttributes = action.GetMethodFilterAttributes();

            var serviceAttributes = action.GetClassFilterAttributes()
                .Where(filter => filter.AllowMultiple ||
                    actionAttributes.Any(mFilter => mFilter.TypeId == filter.TypeId) == false);

            var actionFilters = actionAttributes
                .Select(fiter => new Filter
                {
                    Instance = fiter,
                    FilterScope = (fiter is IAuthorizationFilter) ? FilterScope.Authorization : FilterScope.Method
                });

            var serviceFilters = serviceAttributes
                .Select(fiter => new Filter
                 {
                     Instance = fiter,
                     FilterScope = (fiter is IAuthorizationFilter) ? FilterScope.Authorization : FilterScope.Class
                 });

            var filters = serviceFilters.Concat(actionFilters)
                .OrderBy(filter => filter.FilterScope)
                .ThenBy(filter => filter.Instance.Order);

            return filters;
        }

        /// <summary>
        /// 并将异常传给客户端并调用OnException
        /// </summary>
        /// <param name="exceptionContext">上下文</param>              
        private void RaiseException(ExceptionContext exceptionContext)
        {
            FastTcpCommon.RaiseRemoteException(exceptionContext, this.Serializer);
            this.OnException(exceptionContext);
        }

        /// <summary>
        /// 当操作中遇到处理异常时，将触发此方法
        /// </summary>
        /// <param name="exceptionContext">上下文</param>      
        protected virtual void OnException(ExceptionContext exceptionContext)
        {
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.specialService.Dispose();
                this.serviceActions.Clear();
                this.serviceResolver.Clear();

                this.serviceResolver = null;
                this.specialService = null;
                this.serviceActions = null;
                this.Serializer = null;
            }
        }


        #region 过滤器接口实现
        /// <summary>
        /// 获取或设置排序
        /// </summary>
        public int Order
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// 是否允许多个实例
        /// </summary>
        public bool AllowMultiple
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 授权时触发       
        /// </summary>
        /// <param name="actionContext">上下文</param>       
        /// <returns></returns>
        public virtual void OnAuthorization(ActionContext actionContext)
        {
        }

        /// <summary>
        /// 在执行服务行为前触发       
        /// </summary>
        /// <param name="actionContext">上下文</param>       
        /// <returns></returns>
        public virtual void OnExecuting(ActionContext actionContext)
        {
        }

        /// <summary>
        /// 在执行服务行为后触发
        /// </summary>
        /// <param name="actionContext">上下文</param>      
        public virtual void OnExecuted(ActionContext actionContext)
        {
        }
        #endregion
    }
}
