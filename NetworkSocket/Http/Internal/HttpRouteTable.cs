using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http路由表
    /// </summary>
    internal class HttpRouteTable
    {
        /// <summary>
        /// http行为
        /// </summary>
        private readonly HttpAction[] httpActions;

        /// <summary>
        /// http路由
        /// </summary>
        /// <param name="actions">http行为</param>
        public HttpRouteTable(IEnumerable<HttpAction> actions)
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
            foreach (var action in this.httpActions)
            {
                if (request.HttpMethod != (request.HttpMethod & action.AllowMethod))
                {
                    continue;
                }

                var routeData = action.Route.MatchURL(request.Url);
                if (routeData != null)
                {
                    var actionClone = action.Clone() as HttpAction;
                    actionClone.RouteData = routeData;
                    return actionClone;
                }
            }
            return null;
        }
    }
}
