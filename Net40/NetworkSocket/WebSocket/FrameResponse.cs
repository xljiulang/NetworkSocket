using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示帧类型回复对象
    /// </summary>
    public class FrameResponse : Response
    {
        /// <summary>
        /// 获取帧类型
        /// </summary>
        public FrameCodes Frame { get; private set; }

        /// <summary>
        /// 获取回复内容
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// 回复对象
        /// </summary>
        /// <param name="frame">帧类型</param>
        /// <param name="content">内容</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FrameResponse(FrameCodes frame, byte[] content)
        {
            this.Frame = frame;
            this.Content = content ?? new byte[0];
        }

        /// <summary>
        /// 转换为ByteRange
        /// </summary>
        /// <returns></returns>
        public override ByteRange ToByteRange()
        {
            var builder = new ByteBuilder(Endians.Big);

            builder.Add((byte)((byte)this.Frame + 128));

            if (this.Content.Length > UInt16.MaxValue)
            {
                builder.Add((byte)127);
                builder.Add((ulong)this.Content.Length);
            }
            else if (this.Content.Length > 125)
            {
                builder.Add((byte)126);
                builder.Add((ushort)this.Content.Length);
            }
            else
            {
                builder.Add((byte)this.Content.Length);
            }
            builder.Add(this.Content);
            return builder.ToByteRange();
        }
    }
}
