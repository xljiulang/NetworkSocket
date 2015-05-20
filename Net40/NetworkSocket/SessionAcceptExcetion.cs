using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示接收会话异常
    /// </summary>
    public class SessionAcceptExcetion : Exception
    {
        /// <summary>
        /// 接收会话异常
        /// </summary>
        /// <param name="innerException">内部异常</param>
        public SessionAcceptExcetion(SocketException innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}
