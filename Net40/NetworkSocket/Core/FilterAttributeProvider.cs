using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Core
{
    /// <summary>
    /// Api行为特性过滤器提供者
    /// </summary>
    public class FilterAttributeProvider : IFilterAttributeProvider
    {
        /// <summary>
        /// 服务方法过滤器缓存
        /// </summary>
        private readonly ConcurrentDictionary<ApiAction, IEnumerable<IFilter>> filterCached = new ConcurrentDictionary<ApiAction, IEnumerable<IFilter>>();

        /// <summary>
        /// Api行为特性过滤器提供者
        /// </summary>
        public FilterAttributeProvider()
        {
        }

        /// <summary>
        /// 获取Api行为的特性过滤器     
        /// </summary>
        /// <param name="apiAction">Api行为</param>
        /// <returns></returns>
        public virtual IEnumerable<IFilter> GetActionFilters(ApiAction apiAction)
        {
            return this.filterCached.GetOrAdd(apiAction, action => GetActionFiltersNoCached(action));
        }

        /// <summary>
        /// 获取Api行为的特性过滤器     
        /// </summary>
        /// <param name="action">Api行为</param>
        /// <returns></returns>
        private static IEnumerable<IFilter> GetActionFiltersNoCached(ApiAction action)
        {
            var filters = new List<IFilter>();
            var methodFilters = action.GetMethodFilterAttributes();
            var classFilters = action.GetClassFilterAttributes();

            // 如果类和方法都定义相同的滤过器且不允许多个实例
            // 就只取方法上的过滤器实例
            filters.AddRange(methodFilters);
            foreach (var filter in classFilters)
            {
                if (filter.AllowMultiple || methodFilters.Any(f => f.TypeId == filter.TypeId) == false)
                {
                    filters.Add(filter);
                }
            }
            return filters.OrderBy(item => item.Order).ToArray();
        }
    }
}
