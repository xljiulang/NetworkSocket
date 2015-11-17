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
        /// Api行为
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <exception cref="ArgumentException"></exception>
        public HttpAction(MethodInfo method)
            : base(method)
        {
            var routeAttribute = Attribute.GetCustomAttributes(this.DeclaringService, typeof(RouteAttribute), true).Cast<RouteAttribute>().FirstOrDefault();
            var route = routeAttribute == null ? Regex.Replace(this.DeclaringService.Name, @"Controller$", string.Empty) : routeAttribute.Route;
            this.Route = string.Format("/{0}/{1}", route.Trim('/'), this.ApiName);
        }
    }
}
