using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Json
{
    /// <summary>
    /// 请求上下文
    /// </summary>
    [DebuggerDisplay("Content = {Content}")]
    public class RequestContext
    {
        /// <summary>
        /// 获取或设置服务器对象
        /// </summary>
        public IJsonWebSocketServer WebSocketServer { get; set; }

        /// <summary>
        /// 获取或设置客户端对象
        /// </summary>
        public IClient<Response> Client { get; set; }

        /// <summary>
        /// 获取或设置接收到的数据包
        /// </summary>
        public JsonPacket Packet { get; set; }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Packet.ToString();
        }
    }
}
