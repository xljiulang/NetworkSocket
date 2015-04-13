using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// Flex或Silverlight安全策协议数据封包
    /// </summary>
    public sealed class PolicyPacket : PacketBase
    {
        /// <summary>
        /// 获取或设置二进制数据
        /// </summary>
        public Byte[] Bytes { get; set; }

        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public override byte[] ToByteArray()
        {
            return this.Bytes;
        }

        /// <summary>
        /// 解析收到的数据
        /// </summary>
        /// <param name="builder">数据</param>
        /// <returns></returns>
        public static PolicyPacket From(ByteBuilder builder)
        {
            if (builder.Length == 0)
            {
                return null;
            }
            var bytes = builder.ToArrayThenClear();
            return new PolicyPacket { Bytes = bytes };
        }
    }
}
