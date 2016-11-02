using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service.Fast
{
    /// <summary>
    /// fast协议全局过滤器
    /// </summary>        
    public class FastGlobalFilter : FastFilterAttribute
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            // 标记已处理
            filterContext.ExceptionHandled = true;
        }
    }
}
