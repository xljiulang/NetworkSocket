using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 默认的依赖关系解析程序的实现
    /// </summary>
    internal class DefaultDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// 服务实例反转缓存
        /// </summary>
        private ConcurrentDictionary<Type, object> serviceResolver = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 解析支持任意对象创建的一次注册的服务
        /// </summary>
        /// <param name="serviceType">所请求的服务或对象的类型</param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == null || serviceType.IsAbstract || serviceType.IsInterface)
            {
                return null;
            }
            return this.serviceResolver.GetOrAdd(serviceType, type => Activator.CreateInstance(type));
        }

        /// <summary>
        /// 结束服务实例的生命
        /// </summary>
        /// <param name="service">服务实例</param>
        public void TerminateService(IDisposable service)
        {
        }
    }
}
