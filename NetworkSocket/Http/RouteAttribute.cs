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
    /// 其中{controller}/{action}可以写固定值
    /// *代表匹配多个字，?代表单个字
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// 路由规则正则
        /// </summary>
        private Regex ruleRegex;

        /// <summary>
        /// 获取路由映射 
        /// </summary>
        public string Rule { get; private set; }

        /// <summary>
        /// 获取路由的值
        /// </summary>
        public NameValueCollection RouteDatas { get; private set; }

        /// <summary>
        /// 表示路由映射
        /// 标记：/{segment1}/{segment2}/{controller}/{action}
        /// 其中{controller}/{action}可以写固定值
        /// *代表匹配多个字，?代表单个字
        /// </summary>
        /// <param name="rule">路由规则</param>      
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RouteAttribute(string rule)
        {
            if (string.IsNullOrEmpty(rule))
            {
                throw new ArgumentNullException();
            }

            if (rule.StartsWith("/") == false)
            {
                throw new ArgumentException("route必须以/开始");
            }

            this.Rule = rule;
            this.RouteDatas = new RouteDataCollection();
        }

        /// <summary>
        /// 绑定httpAction
        /// </summary>
        /// <param name="httpAction">http行为</param>
        /// <returns></returns>
        internal RouteAttribute BindHttpAction(HttpAction httpAction)
        {
            this.Rule = Regex.Replace(this.Rule, @"\{controller\}", httpAction.ControllerName, RegexOptions.IgnoreCase);
            this.Rule = Regex.Replace(this.Rule, @"\{action\}", httpAction.ApiName, RegexOptions.IgnoreCase);

            var glob = Regex.Escape(this.Rule).Replace(@"\*", ".*").Replace(@"\?", ".").Replace(@"\{", "{");
            var pattern = Regex.Replace(glob, @"\{\w+\}", (m) =>
            {
                var token = m.Value.TrimStart('{').TrimEnd('}');
                this.RouteDatas.Set(token, null);
                return string.Format(@"(?<{0}>\w+)", token);
            });
            this.ruleRegex = new Regex(pattern, RegexOptions.IgnoreCase);
            return this;
        }

        /// <summary>
        /// 与url进行匹配
        /// 同时更新RouteDatas的值
        /// </summary>
        /// <param name="url">请求的完整url</param>
        /// <returns></returns>
        public virtual bool IsMatch(Uri url)
        {
            var match = this.ruleRegex.Match(url.AbsolutePath);
            if (match.Success == true)
            {
                for (var i = 0; i < this.RouteDatas.Keys.Count; i++)
                {
                    var key = this.RouteDatas.Keys[i];
                    var capture = match.Groups[key];
                    this.RouteDatas.Set(key, capture.Value);
                }
            }
            return match.Success;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Rule;
        }



        /// <summary>
        /// 路由数据集合
        /// </summary>
        private class RouteDataCollection : NameValueCollection
        {
            /// <summary>
            /// 路由数据集合
            /// </summary>
            public RouteDataCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            /// <summary>
            /// 清除
            /// </summary>
            public override void Clear()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// 移除项
            /// </summary>
            /// <param name="name">项名</param>
            public override void Remove(string name)
            {
                throw new NotSupportedException();
            }
        }
    }
}
