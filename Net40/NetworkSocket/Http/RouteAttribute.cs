using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示路由规则
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// 获取或设置路由规则
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// 表示路由规则
        /// </summary>
        /// <param name="route">路由规则</param>
        public RouteAttribute(string route)
        {
            this.Route = route;
        }
    }
}
