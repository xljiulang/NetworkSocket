using NetworkSocket.Fast.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 全局过滤器
    /// </summary>
    public static class GlobalFilters
    {
        /// <summary>
        /// 获取过滤器集合
        /// </summary>
        public static readonly FilterCollection FilterCollection = new FilterCollection();

        /// <summary>
        /// 添加过滤器
        /// </summary>
        /// <param name="filter">过滤器</param>
        public static void Add(IFilter filter)
        {
            GlobalFilters.FilterCollection.Add(filter);
        }
    }

    /// <summary>
    /// 过滤器集合
    /// </summary>
    public class FilterCollection : IEnumerable<Filter>
    {
        /// <summary>
        /// 过滤器列表
        /// </summary>
        private List<Filter> filters = new List<Filter>();

        /// <summary>
        /// 获取IAction过滤器
        /// </summary>
        public IEnumerable<IActionFilter> ActionFilters { get; private set; }

        /// <summary>
        /// 获取IAuthorization过滤器
        /// </summary>
        public IEnumerable<IAuthorizationFilter> AuthorizationFilters { get; private set; }

        /// <summary>
        /// 全局过滤器
        /// </summary>
        internal FilterCollection()
        {
            this.ActionFilters = new List<IActionFilter>();
            this.AuthorizationFilters = new List<IAuthorizationFilter>();
        }

        /// <summary>
        /// 添加过滤器
        /// </summary>
        /// <param name="filter">过滤器</param>
        public void Add(IFilter filter)
        {
            this.filters.Add(new Filter { Instance = filter, FilterScope = FilterScope.Global });
            var actionFilter = filter as IActionFilter;
            var authorizationFilter = filter as IAuthorizationFilter;

            if (actionFilter != null)
            {
                ((IList<IActionFilter>)this.ActionFilters).Add(actionFilter);
            }

            if (authorizationFilter != null)
            {
                ((IList<IAuthorizationFilter>)this.AuthorizationFilters).Add(authorizationFilter);
            }
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Filter> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }
    }
}
