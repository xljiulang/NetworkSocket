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
        /// 据包协议异常
        /// </summary>
        public ProtocolException()
            : base("请求的数据包协议不正确")
        {
        }
    }
}
