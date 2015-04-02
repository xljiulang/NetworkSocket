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
        /// 获取IAuthorization过滤器
        /// </summary>
        public static IEnumerable<IAuthorizationFilter> AuthorizationFilters { get; private set; }

        /// <summary>
        /// 获取IAction过滤器
        /// </summary>
        public static IEnumerable<IActionFilter> ActionFilters { get; private set; }

        /// <summary>
        /// 获取IExceptionFilter过滤器
        /// </summary>
        public static IEnumerable<IExceptionFilter> ExceptionFilters { get; private set; }

        /// <summary>
        /// 全局过滤器
        /// </summary>
        static GlobalFilters()
        {
            AuthorizationFilters = Enumerable.Empty<IAuthorizationFilter>();
            ActionFilters = Enumerable.Empty<IActionFilter>();
            ExceptionFilters = Enumerable.Empty<IExceptionFilter>();
        }

        /// <summary>
        /// 添加过滤器
        /// </summary>
        /// <param name="filter">过滤器</param>
        public static void Add(IFilter filter)
        {
            var actionFilter = filter as IActionFilter;
            var authorizationFilter = filter as IAuthorizationFilter;
            var exceptionFilter = filter as IExceptionFilter;

            if (actionFilter != null)
            {
                ActionFilters = ActionFilters
                    .Concat(new[] { actionFilter })
                    .OrderBy(item => item.Order);
            }

            if (authorizationFilter != null)
            {
                AuthorizationFilters = AuthorizationFilters
                    .Concat(new[] { authorizationFilter })
                    .OrderBy(item => item.Order);
            }

            if (exceptionFilter != null)
            {
                ExceptionFilters = ExceptionFilters
                    .Concat(new[] { exceptionFilter })
                    .OrderBy(item => item.Order);
            }
        }
    }
}
