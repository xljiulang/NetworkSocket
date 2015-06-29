using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NetworkSocket.WebSocket.Fast;

namespace Autofac.NetworkSocket.WebSocket
{
    /// <summary>
    /// Autofac服务行为特性过滤器提供者
    /// </summary>
    internal class AutofacFilterAttributeProvider : FilterAttributeProvider
    {
        /// <summary>
        /// 解析提供者
        /// </summary>
        private AutofacDependencyResolver dependencyResolver;

        /// <summary>
        /// Autofac服务行为特性过滤器提供者
        /// </summary>
        /// <param name="dependencyResolver">解析提供者</param>
        public AutofacFilterAttributeProvider(AutofacDependencyResolver dependencyResolver)
        {
            this.dependencyResolver = dependencyResolver;
        }

        /// <summary>
        /// 获取服务行为的特性过滤器   
        /// 并进行属性注入
        /// </summary>
        /// <param name="fastAction">服务行为</param>
        /// <returns></returns>
        public override IEnumerable<IFilter> GetActionFilters(ApiAction fastAction)
        {
            var filters = base.GetActionFilters(fastAction);
            var lifetimeScope = this.dependencyResolver.CurrentLifetimeScope;

            if (lifetimeScope == null)
            {
                return filters;
            }
            return filters.Select(filter => lifetimeScope.InjectProperties(filter));
        }
    }
}
