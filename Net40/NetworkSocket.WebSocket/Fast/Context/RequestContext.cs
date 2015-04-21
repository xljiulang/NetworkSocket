using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 请求上下文
    /// </summary>
    [DebuggerDisplay("Content = {Content}")]
    public class RequestContext
    {
        /// <summary>
        /// 获取或设置当前所有会话对象
        /// </summary>
        public IEnumerable<FastWebSocketSession> AllSessions { get; set; }

        /// <summary>
        /// 获取或设置会话对象
        /// </summary>
        public FastWebSocketSession Session { get; set; }

        /// <summary>
        /// 获取或设置接收到的数据包
        /// </summary>
        public FastPacket Packet { get; set; }

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
