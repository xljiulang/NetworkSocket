using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 通讯协议数据包抽象类
    /// 要求所有协议实现此类的抽象方法
    /// </summary>
    public abstract class PacketBase
    {
        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToByteArray();
    }
}
