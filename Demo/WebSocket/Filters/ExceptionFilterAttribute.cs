using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket.Filters
{
    /// <summary>
    /// 异常处理过滤器
    /// </summary>
    public class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            Console.WriteLine(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
        }
    }
}
