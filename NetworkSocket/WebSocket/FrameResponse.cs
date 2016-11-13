using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示WebSocket帧类型回复对象
    /// </summary>
    public class FrameResponse : WebsocketResponse
    {
        /// <summary>
        /// 随机数
        /// </summary>
        private static readonly Random ran = new Random();

        /// <summary>
        /// 获取是否结束帧
        /// </summary>
        public bool Fin { get; private set; }

        /// <summary>
        /// 获取帧类型
        /// </summary>
        public FrameCodes Frame { get; private set; }

        /// <summary>
        /// 获取回复内容
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// 构建不分片的回复帧
        /// </summary>
        /// <param name="frame">帧类型</param>
        /// <param name="content">内容</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FrameResponse(FrameCodes frame, byte[] content)
            : this(frame, content, fin: true)
        {
        }

        /// <summary>
        /// 构建回复帧
        /// </summary>
        /// <param name="frame">帧类型</param>
        /// <param name="content">内容</param>
        /// <param name="fin">是否结束帧</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FrameResponse(FrameCodes frame, byte[] content, bool fin)
        {
            this.Frame = frame;
            this.Fin = fin;
            this.Content = content ?? new byte[0];
        }

        /// <summary>
        /// 转换为ArraySegment
        /// </summary>
        /// <param name="mask">是否打码</param>
        /// <returns></returns>
        public override unsafe ArraySegment<byte> ToArraySegment(bool mask)
        {
            var builder = new ByteBuilder(Endians.Big);
           
            ByteBits bits = (byte)this.Frame;
            bits[0] = this.Fin;
            builder.Add(bits);

            if (this.Content.Length > UInt16.MaxValue)
            {
                builder.Add(mask ? byte.MaxValue : (byte)127);
                builder.Add((ulong)this.Content.Length);
            }
            else if (this.Content.Length > 125)
            {
                builder.Add(mask ? (byte)254 : (byte)126);
                builder.Add((ushort)this.Content.Length);
            }
            else
            {
                var len = mask ? (byte)(this.Content.Length + 128) : (byte)this.Content.Length;
                builder.Add(len);
            }

            if (mask == true)
            {
                var maskingKey = ByteConverter.ToBytes(ran.Next(), Endians.Big);
                builder.Add(maskingKey);

                fixed (byte* pcontent = &this.Content[0], pmask = &maskingKey[0])
                {
                    for (var i = 0; i < this.Content.Length; i++)
                    {
                        *(pcontent + i) = (byte)(*(pcontent + i) ^ *(pmask + i % 4));
                    }
                }
            }

            builder.Add(this.Content);
            return builder.ToArraySegment();
        }
    }
}
