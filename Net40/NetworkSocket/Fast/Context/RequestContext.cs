using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 请求上下文
    /// </summary>
    [DebuggerDisplay("Packet = {Packet}")]
    public class RequestContext
    {
        /// <summary>
        /// 获取或设置当前会话对象
        /// </summary>
        public FastSession Session { get; set; }

        /// <summary>
        /// 获取或设置数据包对象
        /// </summary>
        public FastPacket Packet { get; set; }

        /// <summary>
        /// 获取或设置客所有会话对象
        /// </summary>
        public IEnumerable<FastSession> AllSessions { get; set; }

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
