using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 表示上下文对象类空时的异常
    /// </summary>
    public class ContextNullException : Exception
    {
        /// <summary>
        /// 表示上下文对象类空时的异常
        /// </summary>
        /// <param name="message">消息</param>
        public ContextNullException(string message)
            : base(message)
        {
        }
    }
}
