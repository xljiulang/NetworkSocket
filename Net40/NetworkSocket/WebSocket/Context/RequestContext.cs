using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示jsonWebSocket的请求上下文
    /// </summary>
    [DebuggerDisplay("Packet = {Packet}")]
    public class RequestContext
    {
        /// <summary>
        /// 获取当前会话对象
        /// </summary>
        public JsonWebSocketSession Session { get; private set; }

        /// <summary>
        /// 获取数据包对象
        /// </summary>
        public JsonPacket Packet { get; private set; }

        /// <summary>
        /// 获取所有会话对象
        /// </summary>
        public ISessionManager AllSessions { get; private set; }

        /// <summary>
        /// 获取所有JsonWebSocket会话对象
        /// </summary>
        public IEnumerable<JsonWebSocketSession> JsonWebSocketSessions
        {
            get
            {
                return this
                    .AllSessions
                    .FilterWrappers<JsonWebSocketSession>();
            }
        }

        /// <summary>
        /// 请求上下文
        /// </summary>
        /// <param name="session">当前会话对象</param>
        /// <param name="packet">数据包对象</param>
        /// <param name="allSessions">所有会话对象</param>
        internal RequestContext(JsonWebSocketSession session, JsonPacket packet, ISessionManager allSessions)
        {
            this.Session = session;
            this.Packet = packet;
            this.AllSessions = allSessions;
        }

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
