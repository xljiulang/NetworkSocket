using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;
using Autofac;

namespace Server
{
    /// <summary>
    /// Autofac服务行为特性过滤器提供者
    /// </summary>
    public class AutofacFilterAttributeProvider : FilterAttributeProvider
    {
        /// <summary>
        /// 获取服务行为的特性过滤器   
        /// 并进行属性注入
        /// </summary>
        /// <param name="action">服务行为</param>
        /// <returns></returns>
        public override IEnumerable<IFilter> GetActionFilters(FastAction action)
        {
            var filters = base.GetActionFilters(action);
            var lifetimeScope = ((AutofacResolver)DependencyResolver.Current).CurrentLifetimeScope;
            return filters.Select(filter => lifetimeScope.InjectProperties(filter));
        }
    }
}
