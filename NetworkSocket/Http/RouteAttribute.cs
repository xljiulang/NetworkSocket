using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http路由映射 
    /// 标记：/{segment1}/{segment2}/{controller}/{action}
    /// {controller}/{action}可以写固定值
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RouteAttribute : Attribute
    {
        /// <summary>
        /// 路由正则
        /// </summary>
        private Regex regex;

        /// <summary>
        /// 路由项
        /// </summary>
        private readonly IList<string> tokens = new List<string>();

        /// <summary>
        /// 获取路由映射 
        /// </summary>
        public string Route { get; private set; }

        /// <summary>
        /// 获取路由的值
        /// </summary>
        public NameValueCollection RouteDatas { get; private set; }

        /// <summary>
        /// 表示路由映射
        /// 标记：/{segment1}/{segment2}/{controller}/{action}
        /// {controller}/{action}可以写固定值
        /// </summary>
        /// <param name="route">路由规则</param>      
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RouteAttribute(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                throw new ArgumentNullException();
            }

            if (route.StartsWith("/") == false)
            {
                throw new ArgumentException("route必须以/开始");
            }

            this.Route = route;
            this.RouteDatas = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 绑定Action
        /// </summary>
        /// <param name="httpAction">http行为</param>
        internal void BindHttpAction(HttpAction httpAction)
        {
            this.tokens.Clear();
            var pattern = Regex.Replace(this.Route, @"\{\w+\}", (m) =>
            {
                var token = m.Value.TrimStart('{').TrimEnd('}');
                if (token.Equals("controller", StringComparison.OrdinalIgnoreCase))
                {
                    this.Route = Regex.Replace(this.Route, m.Value, httpAction.ControllerName, RegexOptions.IgnoreCase);
                    return httpAction.ControllerName;
                }
                else if (token.Equals("action", StringComparison.OrdinalIgnoreCase))
                {
                    this.Route = Regex.Replace(this.Route, m.Value, httpAction.ApiName, RegexOptions.IgnoreCase);
                    return httpAction.ApiName;
                }
                else
                {
                    this.tokens.Add(token);
                    return string.Format(@"(?<{0}>\w+)", token);
                }
            }, RegexOptions.IgnoreCase);

            this.regex = new Regex(pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 与相应url匹配
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        internal bool IsMatchFor(Uri url)
        {
            this.RouteDatas.Clear();
            var match = this.regex.Match(url.AbsolutePath);

            if (match.Success == false)
            {
                return false;
            }

            foreach (var token in this.tokens)
            {
                var capture = match.Groups[token];
                if (capture != null)
                {
                    this.RouteDatas.Set(token, capture.Value);
                }
            }
            return true;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Route;
        }
    }
}
