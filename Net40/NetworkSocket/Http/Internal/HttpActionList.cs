using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Api行为列表
    /// </summary>
    internal class HttpActionList
    {
        /// <summary>
        /// 全部http方法
        /// </summary>
        private const HttpMethod HttpMethod_ALL = HttpMethod.GET | HttpMethod.POST;

        /// <summary>
        /// Api行为字典
        /// </summary>
        private readonly Dictionary<int, HttpAction> dictionary = new Dictionary<int, HttpAction>();

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

            var key = httpAction.GetHashCode();
            if (this.dictionary.ContainsKey(key) == true)
            {
                throw new ArgumentException(string.Format("Http行为：{0}存在冲突的路由规则", httpAction.Route));
            }
            this.dictionary.Add(key, httpAction);
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
        /// 获取http行为
        /// 如果获取不到则返回null
        /// </summary>
        /// <param name="request">请求上下文</param>
        /// <returns></returns>
        public HttpAction TryGet(HttpRequest request)
        {
            var route = request.Url.AbsolutePath.ToLower();
            var apiAction = default(HttpAction);

            if (request.HttpMethod == HttpMethod.POST)
            {
                var postKey = route.GetHashCode() ^ HttpMethod.POST.GetHashCode();
                if (this.dictionary.TryGetValue(postKey, out apiAction))
                {
                    return apiAction;
                }
            }

            var allKey = route.GetHashCode() ^ HttpMethod_ALL.GetHashCode();
            this.dictionary.TryGetValue(allKey, out apiAction);
            return apiAction;
        }
    }
}
