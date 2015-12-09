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
        /// Http的Api行为 
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <param name="declaringType">声明的类型</param>
        /// <exception cref="ArgumentException"></exception>
        public HttpAction(MethodInfo method, Type declaringType)
            : base(method)
        {
            this.DeclaringService = declaringType;
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
            var routeAttribute = Attribute.GetCustomAttributes(declaringType, typeof(RouteAttribute), false).Cast<RouteAttribute>().FirstOrDefault();
            if (routeAttribute != null)
            {
                route = routeAttribute.Route;
            }
            else
            {
                route = "/" + Regex.Replace(declaringType.Name, @"Controller$", string.Empty, RegexOptions.IgnoreCase);
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
            if (Attribute.IsDefined(method, typeof(HttpPostAttribute), false) == true)
            {
                return HttpMethod.POST;
            }
            return HttpMethod.GET | HttpMethod.POST;
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
