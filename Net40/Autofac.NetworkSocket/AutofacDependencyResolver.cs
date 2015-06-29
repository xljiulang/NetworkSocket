using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;
using Autofac;

namespace Autofac.NetworkSocket
{
    /// <summary>
    /// Autofac依赖注入实现
    /// </summary>
    internal class AutofacDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// Autofac依赖注入容器
        /// </summary>
        private IContainer container;

        /// <summary>
        /// Autofac生命范围
        /// </summary>
        [ThreadStatic]
        private static ILifetimeScope lifeTimeScope;

        /// <summary>
        /// 获取或设置当前的生命范围管理对象
        /// </summary>
        public ILifetimeScope CurrentLifetimeScope
        {
            get
            {
                return lifeTimeScope;
            }
            set
            {
                lifeTimeScope = value;
            }
        }

        /// <summary>
        /// Autofac依赖注入
        /// </summary>
        /// <param name="container">注入容器</param>
        public AutofacDependencyResolver(IContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// 解析支持任意对象创建的一次注册的服务
        /// </summary>
        /// <param name="serviceType">所请求的服务或对象的类型</param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            this.CurrentLifetimeScope = this.container.BeginLifetimeScope();
            return this.CurrentLifetimeScope.Resolve(serviceType);
        }

        /// <summary>
        /// 结束服务实例的生命
        /// </summary>
        /// <param name="service">服务实例</param>
        public void TerminateService(IDisposable service)
        {
            this.CurrentLifetimeScope.Dispose();
            this.CurrentLifetimeScope = null;
        }
    }
}
