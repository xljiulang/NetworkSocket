using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http的Api行为    
    /// </summary>
    [DebuggerDisplay("Route = {Route}")]
    public class HttpAction : ApiAction
    {
        /// <summary>
        /// 获取路由
        /// </summary>
        public RouteAttribute Route { get; private set; }

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
            this.AllowMethod = HttpAction.GetAllowMethod(method);
            this.Route = this.GetRouteAttribute();
        }

        /// <summary>
        /// 获取路由地址
        /// </summary> 
        /// <returns></returns>
        private RouteAttribute GetRouteAttribute()
        {
            var routeAttribute = Attribute.GetCustomAttribute(this.Method.Info, typeof(RouteAttribute), false) as RouteAttribute;
            if (routeAttribute == null)
            {
                routeAttribute = this.DeclaringService.GetCustomAttribute<RouteAttribute>(false);
            }

            if (routeAttribute == null)
            {
                var route = string.Format("/{0}/{1}", this.ControllerName, this.ApiName);
                routeAttribute = new RouteAttribute(route);
            }

            routeAttribute.BindHttpAction(this);
            return routeAttribute;
        }

        /// <summary>
        /// 获取支持的http请求方式
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static HttpMethod GetAllowMethod(MethodInfo method)
        {
            var methodAttribute = Attribute.GetCustomAttribute(method, typeof(HttpMethodFilterAttribute)) as HttpMethodFilterAttribute;
            if (methodAttribute != null)
            {
                return methodAttribute.Method;
            }
            return HttpMethod.ALL;
        }

        /// <summary>
        /// 是否可以创建为一个HttpAction
        /// </summary>
        /// <param name="method">方法 </param>
        /// <returns></returns>
        public static bool IsSupport(MethodInfo method)
        {
            if (method.IsDefined(typeof(NoneActionAttribute)))
            {
                return false;
            }
            return true;
        }
    }
}
