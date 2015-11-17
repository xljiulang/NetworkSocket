using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 帧类型
    /// </summary>
    public enum FrameCodes : byte
    {
        /// <summary>
        /// 后续帧
        /// </summary>
        Continuation = 0,
        /// <summary>
        /// 文本帧
        /// </summary>
        Text = 1,
        /// <summary>
        /// 二进制帧
        /// </summary>
        Binary = 2,
        /// <summary>
        /// 连接关闭
        /// </summary>
        Close = 8,
        /// <summary>
        /// ping
        /// </summary>
        Ping = 9,
        /// <summary>
        /// pong
        /// </summary>
        Pong = 10
    }
}
