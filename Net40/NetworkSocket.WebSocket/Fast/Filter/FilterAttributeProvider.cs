using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// Api行为特性过滤器提供者
    /// </summary>
    public class FilterAttributeProvider : IFilterAttributeProvider
    {
        /// <summary>
        /// 过滤器级别
        /// </summary>
        private enum FilterLevels
        {
            /// <summary>
            /// 权限级过滤
            /// </summary>
            Authorization,

            /// <summary>
            /// 类级过滤
            /// </summary>
            Class,

            /// <summary>
            /// 方法级过滤
            /// </summary>
            Method,
        }

        /// <summary>
        /// 服务方法过滤器缓存
        /// </summary>
        private ConcurrentDictionary<ApiAction, IEnumerable<IFilter>> filterCached = new ConcurrentDictionary<ApiAction, IEnumerable<IFilter>>();

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
            var methodAttributes = action.GetMethodFilterAttributes();

            var classAttributes = action.GetClassFilterAttributes()
                .Where(filter => filter.AllowMultiple || methodAttributes.Any(mFilter => mFilter.TypeId == filter.TypeId) == false);

            var methodFilters = methodAttributes.Select(fiter => new
            {
                Filter = fiter,
                Level = (fiter is IAuthorizationFilter) ? FilterLevels.Authorization : FilterLevels.Method
            });

            var classFilters = classAttributes.Select(fiter => new
            {
                Filter = fiter,
                Level = (fiter is IAuthorizationFilter) ? FilterLevels.Authorization : FilterLevels.Class
            });

            var filters = classFilters.Concat(methodFilters)
                .OrderBy(filter => filter.Level)
                .ThenBy(filter => filter.Filter.Order)
                .Select(filter => filter.Filter);

            return filters;
        }
    }
}
