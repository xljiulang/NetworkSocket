using NetworkSocket.Exceptions;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebsocketChatServer.Filters
{
    /// <summary>
    /// 异常处理
    /// </summary>
    public class ExceptionFilter : JsonWebSocketFilterAttribute
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            // 关闭协议错误的会话
            if (filterContext.Exception is ProtocolException)
            {
                filterContext.Session.Close();
            }
            Console.WriteLine(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;           
        }
    }
}
