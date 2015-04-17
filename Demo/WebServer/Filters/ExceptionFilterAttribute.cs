using NetworkSocket.WebSocket.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServer.Filters
{
    /// <summary>
    /// 异常处理过滤器
    /// </summary>
    public class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is ActionException)
            {
            }
            else if (filterContext.Exception is RemoteException)
            {
            }
            filterContext.ExceptionHandled = true;
        }
    }
}
