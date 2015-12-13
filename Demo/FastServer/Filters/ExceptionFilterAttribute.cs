using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastServer.Filters
{
    /// <summary>
    /// 异常处理过滤器
    /// </summary>        
    public class ExceptionFilterAttribute : FastFilterAttribute
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            // 关闭协议错误的会话
            if (filterContext.Exception is ProtocolException)
            {
                filterContext.Session.Close();
            }

            // 标记已处理
            filterContext.ExceptionHandled = true;
        }
    }
}
