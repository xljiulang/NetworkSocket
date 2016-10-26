using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http路由
    /// </summary>
    internal class HttpRouter
    {
        /// <summary>
        /// http行为
        /// </summary>
        private readonly HttpAction[] httpActions;

        /// <summary>
        /// http路由
        /// </summary>
        /// <param name="actions">http行为</param>
        public HttpRouter(IEnumerable<HttpAction> actions)
        {
            this.httpActions = actions.OrderBy(item => item.AllowMethod).ToArray();
        }

        /// <summary>
        /// 匹配请求找出HttpAction
        /// </summary>
        /// <param name="request">请求</param>
        /// <returns></returns>
        public HttpAction MatchHttpAction(HttpRequest request)
        {
            return this.httpActions.FirstOrDefault(item =>
                request.HttpMethod == (request.HttpMethod & item.AllowMethod) &&
                item.Route.IsMatchFor(request.Url));
        }
    }
}
