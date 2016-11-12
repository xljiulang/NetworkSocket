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
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public abstract class RouteBaseAttribute : Attribute
    {
        /// <summary>
        /// 获取路由映射配置
        /// </summary>
        public string Rule { get; private set; }

        /// <summary>
        /// Http路由映射 
        /// </summary>
        public RouteBaseAttribute(string rule)
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
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="httpAction">http行为</param>    
        /// <returns></returns>
        internal RouteBaseAttribute InitWith(HttpAction httpAction)
        {
            this.Init(httpAction);
            return this;
        }

        /// <summary>
        /// 与url进行匹配
        /// 失败而返回null，成功则返回路由数据集合
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal IEnumerable<RouteData> MatchURL(Uri url)
        {
            IEnumerable<RouteData> routeData;
            if (this.IsMatchURL(url, out routeData) == false)
            {
                return null;
            }
            return routeData == null ? Enumerable.Empty<RouteData>() : routeData;
        }



        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="httpAction">http行为</param>       
        protected abstract void Init(HttpAction httpAction);


        /// <summary>
        /// 与url进行匹配
        /// 生成路由数据集合
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="routeData">路由数据集合</param>
        /// <returns></returns>
        protected abstract bool IsMatchURL(Uri url, out IEnumerable<RouteData> routeData);
    }
}
