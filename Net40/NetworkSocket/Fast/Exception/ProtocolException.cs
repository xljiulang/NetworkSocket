using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示数据包协议异常
    /// </summary>
    public class ProtocolException : Exception
    {
        /// <summary>
        /// 数据包协议异常
        /// </summary>
        public ProtocolException()
            : base("数据包协议不正确或数据包太大")
        {
        }
    }
}
