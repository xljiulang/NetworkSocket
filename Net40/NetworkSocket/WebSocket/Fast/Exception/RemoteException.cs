using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 表示远程端Api行为异常
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Message = {Message}")]
    public class RemoteException : Exception
    {
        /// <summary>
        /// 远程端Api行为异常
        /// </summary>       
        /// <param name="message">异常信息</param>
        public RemoteException(string message)
            : base(message)
        {
        }
    }
}
