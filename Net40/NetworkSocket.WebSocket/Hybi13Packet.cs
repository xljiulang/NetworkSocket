using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// RFC6455协议封包
    /// </summary>
    public abstract class Hybi13Packet : PacketBase
    {
        /// <summary>
        /// 发送前或接收后已处理的数据
        /// </summary>
        public byte[] Bytes { get; protected set; }

        /// <summary>
        /// 帧类型
        /// </summary>
        public FrameTypes FrameType { get; protected set; }

        /// <summary>
        /// 将数据作帧处理
        /// </summary>
        /// <param name="bytes">原始数据</param>
        /// <param name="frameType">帧类型</param>
        /// <returns></returns>
        protected byte[] FrameBuffer(byte[] bytes, FrameTypes frameType)
        {
            var builder = new ByteBuilder();

            builder.Add((byte)((byte)frameType + 128));

            if (bytes.Length > UInt16.MaxValue)
            {
                builder.Add((byte)127);
                builder.Add((ulong)bytes.Length, Endians.Big);
            }
            else if (bytes.Length > 125)
            {
                builder.Add((byte)126);
                builder.Add((ushort)bytes.Length, Endians.Big);
            }
            else
            {
                builder.Add((byte)bytes.Length);
            }
            builder.Add(bytes);
            return builder.ToArray();
        }

        /// <summary>
        /// 将封包转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public override byte[] ToByteArray()
        {
            return this.Bytes == null ? new byte[0] : this.Bytes;
        }
    }
}
