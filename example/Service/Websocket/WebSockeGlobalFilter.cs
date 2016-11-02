using NetworkSocket.Exceptions;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service.Websocket
{
    /// <summary>
    /// jsonWebSocke协议全局过滤器
    /// </summary>
    public class WebSockeGlobalFilter : JsonWebSocketFilterAttribute
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            Console.WriteLine(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
        }
    }
}
