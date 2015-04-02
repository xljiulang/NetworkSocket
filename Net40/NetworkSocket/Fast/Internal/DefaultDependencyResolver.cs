using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Internal
{
    /// <summary>
    /// 默认的依赖关系解析程序的实现
    /// </summary>
    internal class DefaultDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// 服务实例反转缓存
        /// </summary>
        private ConcurrentDictionary<Type, FastServiceBase> serviceResolver = new ConcurrentDictionary<Type, FastServiceBase>();

        /// <summary>
        /// 解析支持任意对象创建的一次注册的服务
        /// </summary>
        /// <param name="serviceType">所请求的服务或对象的类型</param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            return this.serviceResolver.GetOrAdd(serviceType, type => Activator.CreateInstance(type) as FastServiceBase);
        }

        /// <summary>
        /// 获取是否支持自动管理服务的生命周期
        /// 返回true则TerminateService会被调用
        /// </summary>
        public bool SupportLifetimeManage
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 结束服务实例的生命
        /// </summary>
        /// <param name="service">服务实例</param>
        public void TerminateService(IDisposable service)
        {
            throw new NotImplementedException();
        }
    }
}
