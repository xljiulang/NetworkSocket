using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 通讯发送的数据包抽象类   
    /// </summary>
    public abstract class PacketBase
    {
        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToBytes();
    }
}
