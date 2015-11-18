using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// Http行为    
    /// </summary>
    [DebuggerDisplay("ApiName = {ApiName}")]
    public class HttpAction : ApiAction
    {
        /// <summary>
        /// 获取路由规则
        /// </summary>
        public string Route { get; private set; }

        /// <summary>
        /// 获取是允许的请求方式
        /// </summary>
        public HttpMethod AllowMethod { get; private set; }

        /// <summary>
        /// Api行为
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <param name="declaringType">声明的类型</param>
        /// <exception cref="ArgumentException"></exception>
        public HttpAction(MethodInfo method, Type declaringType)
            : base(method)
        {
            this.DeclaringService = declaringType;
            var routeAttribute = Attribute.GetCustomAttributes(declaringType, typeof(RouteAttribute), true).Cast<RouteAttribute>().FirstOrDefault();
            var route = routeAttribute == null ? Regex.Replace(declaringType.Name, @"Controller$", string.Empty, RegexOptions.IgnoreCase) : routeAttribute.Route;
            this.Route = string.Format("/{0}/{1}", route.Trim('/'), this.ApiName).ToLower();
         
            if (Attribute.IsDefined(method, typeof(HttpPostAttribute), false) == true)
            {
                this.AllowMethod = HttpMethod.POST;
            }
            else
            {
                this.AllowMethod = HttpMethod.GET | HttpMethod.POST | HttpMethod.PUT | HttpMethod.DELETE;
            }
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Route.GetHashCode() ^ this.AllowMethod.GetHashCode();
        }

        /// <summary>
        /// 比较是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return obj.GetHashCode() == this.GetHashCode() && obj is HttpAction;
        }
    }
}
