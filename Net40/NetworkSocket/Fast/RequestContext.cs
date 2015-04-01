using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 请求上下文
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// 获取或设置客户端对象
        /// </summary>
        public SocketAsync<FastPacket> Client { get; set; }

        /// <summary>
        /// 获取或设置数据包对象
        /// </summary>
        public FastPacket Packet { get; set; }
    }
}
