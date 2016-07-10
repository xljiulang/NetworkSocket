using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http的Api行为    
    /// </summary>
    [DebuggerDisplay("Route = {Route}")]
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
        /// 获取方法的名称
        /// </summary>
        public string ActionName { get; protected set; }


        /// <summary>
        /// 获取控制器名称
        /// </summary>
        public string ControllerName { get; protected set; }


        /// <summary>
        /// Http的Api行为 
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <param name="declaringType">声明的类型</param>
        /// <exception cref="ArgumentException"></exception>
        public HttpAction(MethodInfo method, Type declaringType)
            : base(method)
        {
            this.DeclaringService = declaringType;
            this.ActionName = this.Method.Name;
            this.ControllerName = Regex.Replace(declaringType.Name, @"Controller$", string.Empty, RegexOptions.IgnoreCase);
            this.AllowMethod = this.GetAllowMethod(method);
            this.Route = this.GetRoute(declaringType);
        }

        /// <summary>
        /// 获取路由地址
        /// </summary>
        /// <param name="declaringType"></param>
        /// <returns></returns>
        private string GetRoute(Type declaringType)
        {
            var route = string.Empty;
            var routeAttribute = Attribute
                .GetCustomAttributes(declaringType, typeof(RouteAttribute), false)
                .Cast<RouteAttribute>()
                .FirstOrDefault();

            if (routeAttribute != null)
            {
                route = routeAttribute.Route;
            }
            else
            {
                route = "/" + this.ControllerName;
            }

            if (route.Length > 1 && route.EndsWith("/") == false)
            {
                route = route + "/";
            }
            return (route + this.ApiName).ToLower();
        }


        /// <summary>
        /// 获取支持的http请求方式
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private HttpMethod GetAllowMethod(MethodInfo method)
        {
            var methodAttribute = Attribute.GetCustomAttribute(method, typeof(HttpMethodFilterAttribute)) as HttpMethodFilterAttribute;
            if (methodAttribute != null)
            {
                return methodAttribute.Method;
            }
            return HttpMethod.ALL;
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
