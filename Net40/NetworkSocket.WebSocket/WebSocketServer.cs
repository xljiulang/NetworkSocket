using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// WebSocket服务
    /// 只支持 RFC 6455 协议
    /// </summary>
    public class WebSocketServer : WebSocketServerBase<WebSocketSession>
    {
        /// <summary>
        /// 实例化会话对象
        /// </summary>
        /// <returns></returns>
        protected override WebSocketSession OnCreateSession()
        {
            return new WebSocketSession();
        }
    }
}
