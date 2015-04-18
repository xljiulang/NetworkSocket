using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 权限过虑器
    /// </summary>
    public interface IAuthorizationFilter : IFilter
    {
        /// <summary>
        /// 授权时触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        void OnAuthorization(ActionContext filterContext);
    }
}
