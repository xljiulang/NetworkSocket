using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpServer.Filters
{
    /// <summary>
    /// 异常处理过滤器
    /// </summary>
    public class ExceptionFilterAttribute : HttpFilterAttribute
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ContentResult("异常处理完成 ..");
        }
    }
}
