using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixServer.GlobalFilters
{
    /// <summary>
    /// htp协议全局过滤器
    /// </summary>
    public class HttpGlobalFilter : HttpFilterAttribute
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ContentResult(filterContext.Exception.Message);
        }
    }
}
