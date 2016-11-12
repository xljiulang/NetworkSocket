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
    public sealed class HttpAction : ApiAction, ICloneable<HttpAction>
    {
        /// <summary>
        /// 获取路由映射
        /// </summary>
        public RouteBaseAttribute Route { get; private set; }

        /// <summary>
        /// 获取路由映射数据
        /// </summary>
        public RouteDataCollection RouteDatas { get; private set; }

        /// <summary>
        /// 获取是允许的请求方式
        /// </summary>
        public HttpMethod AllowMethod { get; private set; }

        /// <summary>
        /// 获取控制器名称
        /// </summary>
        public string ControllerName { get; private set; }

        /// <summary>
        /// Http的Api行为 
        /// </summary>
        private HttpAction()
        {
        }

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
            this.ControllerName = Regex.Replace(declaringType.Name, @"Controller$", string.Empty, RegexOptions.IgnoreCase);
            this.AllowMethod = HttpAction.GetAllowMethod(method);
            this.Route = this.GetRouteAttribute();
            this.RouteDatas = new RouteDataCollection();
        }

        /// <summary>
        /// 获取路由地址
        /// </summary> 
        /// <returns></returns>
        private RouteBaseAttribute GetRouteAttribute()
        {
            var route = this.Method.Info.GetCustomAttribute<RouteBaseAttribute>(false);
            if (route == null)
            {
                route = this.DeclaringService.GetCustomAttribute<RouteBaseAttribute>(false);
            }

            if (route == null)
            {
                var rule = string.Format("/{0}/{1}", this.ControllerName, this.ApiName);
                route = new RouteAttribute(rule);
            }
            return route.InitWith(this);
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
        /// 返回是否是一个HttpAction
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns></returns>
        public static bool IsHttpAction(MethodInfo method)
        {
            if (method.IsDefined(typeof(NoneActionAttribute)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 克隆构造器
        /// </summary>
        /// <returns></returns>
        HttpAction ICloneable<HttpAction>.CloneConstructor()
        {
            return new HttpAction
            {
                ApiName = this.ApiName,
                Method = this.Method,
                IsTaskReturn = this.IsTaskReturn,
                IsVoidReturn = this.IsVoidReturn,
                DeclaringService = this.DeclaringService,
                Parameters = this.Parameters.Select(p => new ApiParameter(p.Info)).ToArray(),
                AllowMethod = this.AllowMethod,
                ControllerName = this.ControllerName,
                Route = this.Route,
                RouteDatas = new RouteDataCollection()
            };
        }
    }
}
