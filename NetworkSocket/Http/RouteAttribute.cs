using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http路由映射 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RouteAttribute : Attribute
    {
        /// <summary>
        /// 获取路由映射 
        /// </summary>
        public string Route { get; private set; }

        /// <summary>
        /// 表示路由映射
        /// 以/开始
        /// </summary>
        /// <param name="route">路由映射</param>
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
        }
    }
}
