using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 默认提供的Api行为特性过滤器提供者
    /// </summary>
    internal class DefaultFilterAttributeProvider : IFilterAttributeProvider
    {
        /// <summary>
        /// 获取Api行为的特性过滤器     
        /// </summary>
        /// <param name="apiAction">Api行为</param>
        /// <returns></returns>
        public IEnumerable<IFilter> GetActionFilters(ApiAction apiAction)
        {
            var paramtersFilters = apiAction.GetParametersFilterAttributes(cache: true);
            var methodFilters = apiAction.GetMethodFilterAttributes();
            var classFilters = apiAction.GetClassFilterAttributes()
                .Where(cf => cf.AllowMultiple || methodFilters.Any(mf => mf.TypeId == cf.TypeId) == false);

            var allFilters = paramtersFilters.Concat(methodFilters).Concat(classFilters).OrderBy(f => f.Order).ToArray();
            return allFilters;
        }
    }
}
