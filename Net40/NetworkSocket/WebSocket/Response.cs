using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示回复对象抽象类
    /// </summary>
    public abstract class Response
    {
        /// <summary>
        /// 转换ByteArray类型
        /// </summary>
        /// <returns></returns>
        public abstract ByteRange ToByteRange();
    }
}
