using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Api行为列表
    /// </summary>
    internal class HttpActionList
    {
        /// <summary>
        /// Api行为字典
        /// </summary>
        private Dictionary<string, HttpAction> dictionary = new Dictionary<string, HttpAction>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 添加Api行为
        /// </summary>
        /// <param name="httpAction">Api行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Add(HttpAction httpAction)
        {
            if (httpAction == null)
            {
                throw new ArgumentNullException("apiAction");
            }

            if (this.dictionary.ContainsKey(httpAction.Route))
            {
                throw new ArgumentException(string.Format("Http行为：{0}存在冲突的路由规则", httpAction.Route));
            }
            this.dictionary.Add(httpAction.Route, httpAction);
        }

        /// <summary>
        /// 添加Api行为
        /// </summary>
        /// <param name="apiActions">Api行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddRange(IEnumerable<HttpAction> apiActions)
        {
            foreach (var action in apiActions)
            {
                this.Add(action);
            }
        }

        /// <summary>
        /// 获取Api行为
        /// 如果获取不到则返回null
        /// </summary>
        /// <param name="route">路由规则</param>
        /// <returns></returns>
        public HttpAction TryGet(string route)
        {
            HttpAction apiAction;
            if (this.dictionary.TryGetValue(route, out apiAction))
            {
                return apiAction;
            }
            return null;
        }
    }
}
