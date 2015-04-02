using NetworkSocket.Fast.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 服务行为特性过滤器提供者
    /// </summary>
    public class FilterAttributeProvider : IFilterAttributeProvider
    {
        /// <summary>
        /// 服务行为特性过滤器提供者
        /// </summary>
        public FilterAttributeProvider()
        {
        }

        /// <summary>
        /// 获取服务行为的特性过滤器     
        /// </summary>
        /// <param name="action">服务行为</param>
        /// <returns></returns>
        public virtual IEnumerable<IFilter> GetActionFilters(FastAction action)
        {
            var methodAttributes = action.GetMethodFilterAttributes();

            var classAttributes = action.GetClassFilterAttributes()
                .Where(filter => filter.AllowMultiple || methodAttributes.Any(mFilter => mFilter.TypeId == filter.TypeId) == false);

            var methodFilters = methodAttributes.Select(fiter => new
            {
                Filter = fiter,
                Level = (fiter is IAuthorizationFilter) ? FilterLevel.Authorization : FilterLevel.Method
            });

            var classFilters = classAttributes.Select(fiter => new
            {
                Filter = fiter,
                Level = (fiter is IAuthorizationFilter) ? FilterLevel.Authorization : FilterLevel.Class
            });

            var filters = classFilters.Concat(methodFilters)
                .OrderBy(filter => filter.Level)
                .ThenBy(filter => filter.Filter.Order)
                .Select(filter => filter.Filter);

            return filters;
        }
    }
}
