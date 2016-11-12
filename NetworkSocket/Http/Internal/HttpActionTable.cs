using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http行为表
    /// </summary>
    internal class HttpActionTable
    {
        /// <summary>
        /// http行为
        /// </summary>
        private readonly HttpAction[] httpActions;

        /// <summary>
        /// http行为表
        /// </summary>
        /// <param name="actions">http行为</param>
        public HttpActionTable(IEnumerable<HttpAction> actions)
        {
            this.httpActions = actions.OrderBy(item => item.AllowMethod).ToArray();
        }

        /// <summary>       
        /// 如果匹配请求，则返回其克隆体
        /// 否则返回null
        /// </summary>
        /// <param name="request">请求</param>
        /// <returns></returns>
        public HttpAction TryGetAndClone(HttpRequest request)
        {
            foreach (var action in this.httpActions)
            {
                var clone = this.MatchAndClone(action, request);
                if (clone != null)
                {
                    return clone;
                }
            }
            return null;
        }

        /// <summary>
        /// 匹配请求，如果成功则返回克隆
        /// </summary>
        /// <param name="action">http行为</param>
        /// <param name="request">请求</param>
        /// <returns></returns>
        private HttpAction MatchAndClone(HttpAction action, HttpRequest request)
        {
            var method = request.HttpMethod & action.AllowMethod;
            if (request.HttpMethod != method)
            {
                return null;
            }
            var routeData = action.Route.MatchURL(request.Url);
            if (routeData == null)
            {
                return null;
            }

            var clone = ((ICloneable<HttpAction>)action).CloneConstructor();
            foreach (var kv in routeData)
            {
                clone.RouteDatas.Set(kv.Key, kv.Value);
            }
            return clone;
        }
    }
}
