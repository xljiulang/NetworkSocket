using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// 二进制数据封包
    /// </summary>
    public sealed class BinaryPacket : PacketBase
    {
        /// <summary>
        /// 获取或设置二进制数据
        /// </summary>
        public Byte[] Bytes { get; set; }

        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            return this.Bytes;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="bytes">数据</param>
        /// <returns></returns>
        public static implicit operator BinaryPacket(byte[] bytes)
        {
            return new BinaryPacket { Bytes = bytes };
        }
    }
}
