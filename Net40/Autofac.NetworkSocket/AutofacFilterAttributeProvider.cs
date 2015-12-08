using Autofac;
using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.NetworkSocket
{
    /// <summary>
    /// Autofac服务行为特性过滤器提供者
    /// </summary>
    internal class AutofacFilterAttributeProvider : IFilterAttributeProvider
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
        /// <param name="apiAction">服务行为</param>
        /// <returns></returns>
        public IEnumerable<IFilter> GetActionFilters(ApiAction action)
        {
            var methodFilters = action.GetMethodFilterAttributes();
            var classFilters = action.GetClassFilterAttributes().Where(cf => cf.AllowMultiple || methodFilters.Any(mf => mf.TypeId == cf.TypeId) == false);
            var filters = methodFilters.Concat(classFilters).OrderBy(f => f.Order);

            var lifetimeScope = this.dependencyResolver.CurrentLifetimeScope;
            if (lifetimeScope == null)
            {
                return filters;
            }
            return filters.Select(f => lifetimeScope.InjectProperties(f));
        }
    }
}

