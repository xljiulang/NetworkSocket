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
    public class FilterCollection : IEnumerable<IFilter>
    {
        /// <summary>
        /// 过滤器列表
        /// </summary>
        private List<IFilter> filters = new List<IFilter>();

        /// <summary>
        /// 获取IAction过滤器
        /// </summary>
        public IEnumerable<IActionFilter> ActionFilters { get; private set; }

        /// <summary>
        /// 获取IAuthorization过滤器
        /// </summary>
        public IEnumerable<IAuthorizationFilter> AuthorizationFilters { get; private set; }

        /// <summary>
        /// 获取IExceptionFilter过滤器
        /// </summary>
        public IEnumerable<IExceptionFilter> ExceptionFilters { get; private set; }

        /// <summary>
        /// 全局过滤器
        /// </summary>
        internal FilterCollection()
        {
            this.ActionFilters = Enumerable.Empty<IActionFilter>();
            this.AuthorizationFilters = Enumerable.Empty<IAuthorizationFilter>();
            this.ExceptionFilters = Enumerable.Empty<IExceptionFilter>();
        }

        /// <summary>
        /// 添加过滤器
        /// </summary>
        /// <param name="filter">过滤器</param>
        public void Add(IFilter filter)
        {
            this.filters.Add(filter);
            var actionFilter = filter as IActionFilter;
            var authorizationFilter = filter as IAuthorizationFilter;
            var exceptionFilter = filter as IExceptionFilter;

            if (actionFilter != null)
            {
                this.ActionFilters = this.ActionFilters
                    .Concat(new[] { actionFilter })
                    .OrderBy(item => item.Order);
            }

            if (authorizationFilter != null)
            {
                this.AuthorizationFilters = this.AuthorizationFilters
                    .Concat(new[] { authorizationFilter })
                    .OrderBy(item => item.Order);
            }

            if (exceptionFilter != null)
            {
                this.ExceptionFilters = this.ExceptionFilters
                    .Concat(new[] { exceptionFilter })
                    .OrderBy(item => item.Order);
            }
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IFilter> GetEnumerator()
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
