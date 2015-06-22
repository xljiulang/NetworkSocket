using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 表示全局过滤器
    /// </summary>
    public class GlobalFilters
    {
        /// <summary>
        /// 获取IAction过滤器
        /// </summary>
        public IEnumerable<IActionFilter> ActionFilters { get; private set; }

        /// <summary>
        /// 获取IExceptionFilter过滤器
        /// </summary>
        public IEnumerable<IExceptionFilter> ExceptionFilters { get; private set; }

        /// <summary>
        /// 获取IAuthorization过滤器
        /// </summary>
        public IEnumerable<IAuthorizationFilter> AuthorizationFilters { get; private set; }

        /// <summary>
        /// 全局过滤器
        /// </summary>
        public GlobalFilters()
        {
            this.ActionFilters = Enumerable.Empty<IActionFilter>();
            this.ExceptionFilters = Enumerable.Empty<IExceptionFilter>();
            this.AuthorizationFilters = Enumerable.Empty<IAuthorizationFilter>();
        }

        /// <summary>
        /// 移除过滤器类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Remove<T>() where T : class, IFilter
        {
            this.ActionFilters = this.ActionFilters.Where(item => item.GetType() != typeof(T));
            this.ExceptionFilters = this.ExceptionFilters.Where(item => item.GetType() != typeof(T));
            this.AuthorizationFilters = this.AuthorizationFilters.Where(item => item.GetType() != typeof(T));
        }

        /// <summary>
        /// 添加过滤器并按Order字段排序
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool Add(IFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException();
            }

            var actionFilter = filter as IActionFilter;
            if (actionFilter != null)
            {
                if (actionFilter.AllowMultiple == false && this.ActionFilters.Any(item => item.GetType() == actionFilter.GetType()))
                {
                    return false;
                }
                this.ActionFilters = this.ActionFilters.Concat(new[] { actionFilter }).OrderBy(item => item.Order);
            }

            var authorizationFilter = filter as IAuthorizationFilter;
            if (authorizationFilter != null)
            {
                if (authorizationFilter.AllowMultiple == false && this.AuthorizationFilters.Any(item => item.GetType() == authorizationFilter.GetType()))
                {
                    return false;
                }
                this.AuthorizationFilters = this.AuthorizationFilters.Concat(new[] { authorizationFilter }).OrderBy(item => item.Order);
            }

            var exceptionFilter = filter as IExceptionFilter;
            if (exceptionFilter != null)
            {
                if (exceptionFilter.AllowMultiple == false && this.ExceptionFilters.Any(item => item.GetType() == exceptionFilter.GetType()))
                {
                    return false;
                }
                this.ExceptionFilters = this.ExceptionFilters.Concat(new[] { exceptionFilter }).OrderBy(item => item.Order);
            }

            return true;
        }
    }
}
