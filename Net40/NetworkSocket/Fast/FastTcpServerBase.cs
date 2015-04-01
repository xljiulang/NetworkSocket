using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using NetworkSocket.Fast.Methods;
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
    public abstract class FastTcpServerBase : TcpServerBase<FastPacket>
    {
        /// <summary>
        /// 所有服务方法
        /// </summary>
        private List<ServiceMethod> serviceMethodList;

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
            this.serviceMethodList = new List<ServiceMethod>();
            this.specialService = new SpecialService();
            this.serviceResolver = new ConcurrentDictionary<Type, FastServiceBase>();
            this.Serializer = new DefaultSerializer();

            var specialMethods = FastTcpCommon.GetServiceMethodList(typeof(SpecialService));
            this.serviceMethodList.AddRange(specialMethods);
        }

        /// <summary>
        /// 绑定本程序集所有的服务
        /// </summary>
        public void BindService()
        {
            var services = this.GetType().Assembly.GetTypes().Where(item => typeof(FastServiceBase).IsAssignableFrom(item));
            this.BindService(services);
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
                var methods = FastTcpCommon.GetServiceMethodList(type);
                this.serviceMethodList.AddRange(methods);
            }

            this.CheckServiceMethodRepeatCommand(this.serviceMethodList);
            foreach (var method in this.serviceMethodList)
            {
                this.CheckForServiceMethodContract(method);
            }
        }


        /// <summary>
        /// 检测服务方法是否有声明相同的Command
        /// </summary>
        /// <param name="methods"></param>
        private void CheckServiceMethodRepeatCommand(IEnumerable<ServiceMethod> methods)
        {
            var group = methods.GroupBy(item => item.ServiceAttribute.Command).FirstOrDefault(g => g.Count() > 1);
            if (group != null)
            {
                throw new Exception(string.Format("Command为{0}不允许被重复使用", group.Key));
            }
        }

        /// <summary>
        /// 检测服务方法的声明和参数
        /// </summary>
        /// <param name="method">服务方法</param>
        private void CheckForServiceMethodContract(ServiceMethod method)
        {
            if (Enum.IsDefined(typeof(SpecialCommands), method.ServiceAttribute.Command) && method.IsDefined(typeof(SpecialServiceAttribute), true) == false)
            {
                throw new Exception(string.Format("服务方法{0}的Command是不允许使用的SpecialCommand命令", method.Method.Name));
            }

            if (method.ParameterTypes.Length == 0 || method.ParameterTypes.First().Equals(typeof(SocketAsync<FastPacket>)) == false)
            {
                throw new Exception(string.Format("服务方法{0}的第一个参数必须是SocketAsync<FastPacket>类型", method.Method.Name));
            }

            if (method.ServiceAttribute.Implement == Implements.Remote)
            {
                var returnType = method.Method.ReturnType;
                var isTask = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);
                var isVoid = method.HasReturn == false;
                if ((isVoid || isTask) == false)
                {
                    throw new Exception(string.Format("服务方法{0}的的返回类型必须是Task<T>类型", method.Method.Name));
                }
            }
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
                this.OnException(client, exception);
            }
        }

        /// <summary>
        /// 处理正常数据包
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        private void ProcessNormalPacket(SocketAsync<FastPacket> client, FastPacket packet)
        {
            var method = this.serviceMethodList.Find(item => item.ServiceAttribute.Command == packet.Command);
            if (method == null)
            {
                var exception = new Exception(string.Format("命令为{0}的服务方法不存在", packet.Command));
                this.RaiseException(client, packet, exception);
                return;
            }

            var isSpecail = true;
            FastServiceBase fastService = this.specialService;

            if (Enum.IsDefined(typeof(SpecialCommands), packet.Command) == false)
            {
                fastService = this.GetService(method.DeclaringType);
                isSpecail = false;
            }

            if (fastService == null)
            {
                var ex = new Exception(string.Format("无法获取类型{0}的实例", method.Method.DeclaringType));
                this.RaiseException(client, packet, ex);
                return;
            }

            // 处理数据包
            fastService.Serializer = this.Serializer;
            fastService.GetFilters = this.GetFilters;
            fastService.ProcessPacket(client, packet, method);

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
        /// 获取过滤器
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns></returns>
        protected virtual IEnumerable<Filter> GetFilters(MethodInfo method)
        {
            var methodAttributes = Attribute.GetCustomAttributes(method, typeof(FilterAttribute), true)
                .Cast<FilterAttribute>();

            var classAttributes = Attribute.GetCustomAttributes(method.DeclaringType, typeof(FilterAttribute), true)
                .Cast<FilterAttribute>()
                .Where(filter => filter.AllowMultiple || methodAttributes.Any(mFilter => mFilter.TypeId == filter.TypeId) == false);


            var methodFilters = methodAttributes
                .Select(fiter => new Filter
                {
                    Instance = fiter,
                    FilterScope = (fiter is IAuthorizationFilter) ? FilterScope.Authorization : FilterScope.ActionMethod
                });

            var classFilters = classAttributes
                .Select(fiter => new Filter
                 {
                     Instance = fiter,
                     FilterScope = (fiter is IAuthorizationFilter) ? FilterScope.Authorization : FilterScope.ActionClass
                 });

            return methodFilters.Concat(classFilters).OrderBy(filter => filter.FilterScope).ThenBy(filter => filter.Instance.Order);
        }

        /// <summary>
        /// 并将异常传给客户端并调用OnException
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">封包</param>
        /// <param name="exception">异常</param>         
        private void RaiseException(SocketAsync<FastPacket> client, FastPacket packet, Exception exception)
        {
            FastTcpCommon.RaiseRemoteException(client, packet, exception, this.Serializer);
            this.OnException(client, exception);
        }

        /// <summary>
        /// 当操作中遇到处理异常时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="exception">异常</param>
        protected virtual void OnException(SocketAsync<FastPacket> client, Exception exception)
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
                this.serviceMethodList.Clear();
                this.serviceResolver.Clear();

                this.serviceResolver = null;
                this.specialService = null;
                this.serviceMethodList = null;
                this.Serializer = null;
            }
        }
    }
}
