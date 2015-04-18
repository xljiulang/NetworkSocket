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
        /// 回复对象
        /// </summary>
        /// <param name="frame">帧类型</param>
        /// <param name="content">内容</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FrameResponse(FrameCodes frame, byte[] content)
            : base(content)
        {
            this.Frame = frame;
        }

        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            var builder = new ByteBuilder();

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
            return builder.ToArray();
        }
    }
}
