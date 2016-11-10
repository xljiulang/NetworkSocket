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
    public sealed class RouteAttribute : RouteBaseAttribute
    {
        /// <summary>
        /// 路由规则正则
        /// </summary>
        private Regex ruleRegex;

        /// <summary>
        /// 路由规则
        /// </summary>
        private string fixRule;

        /// <summary>
        /// tokens
        /// </summary>
        private readonly List<string> tokens = new List<string>();

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
            : base(rule)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="httpAction">http行为</param> 
        protected override void Init(HttpAction httpAction)
        {
            this.fixRule = Regex.Replace(this.Rule, @"\{controller\}", httpAction.ControllerName, RegexOptions.IgnoreCase);
            this.fixRule = Regex.Replace(this.fixRule, @"\{action\}", httpAction.ApiName, RegexOptions.IgnoreCase);

            var glob = Regex.Escape(this.fixRule).Replace(@"\*", ".*").Replace(@"\?", ".").Replace(@"\{", "{");
            var pattern = Regex.Replace(glob, @"\{\w+\}", (m) =>
            {
                var token = m.Value.TrimStart('{').TrimEnd('}');
                this.tokens.Add(token);
                return string.Format(@"(?<{0}>\w+)", token);
            });
            this.ruleRegex = new Regex("^" + pattern + "$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 与url进行匹配
        /// 生成路由数据集合
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="routeData">路由数据集合</param>
        protected override bool IsMatchURL(Uri url, out IEnumerable<KeyValuePair<string, string>> routeData)
        {
            var match = this.ruleRegex.Match(url.AbsolutePath);
            if (match.Success == true)
            {
                routeData = this.tokens.Select(key => new KeyValuePair<string, string>(key, match.Groups[key].Value));
                return true;
            }
            else
            {
                routeData = null;
                return false;
            }
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.fixRule;
        }
    }
}
