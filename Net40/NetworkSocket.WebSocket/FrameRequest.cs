using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示请求帧
    /// </summary>
    public class FrameRequest : Request
    {
        /// <summary>
        /// 获取帧类型
        /// </summary>
        public Frames Frame { get; private set; }

        /// <summary>
        /// 获取请求帧的内容
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// 解析请求的数据
        /// 返回请求数据包
        /// </summary>
        /// <param name="builder">所有收到的数据</param>
        /// <param name="contentBuilder">用于保存数理后的数据</param>
        /// <returns></returns>
        public unsafe static FrameRequest From(ByteBuilder builder, ByteBuilder contentBuilder)
        {
            if (builder.Length < 2)
            {
                return null;
            }

            var isFinal = (builder.ToByte(0) & 128) != 0;
            var reservedBits = builder.ToByte(0) & 112;
            var frameType = (Frames)(builder.ToByte(0) & 15);
            var isMasked = (builder.ToByte(1) & 128) != 0;
            var length = builder.ToByte(1) & 127;

            if (isMasked == false || Enum.IsDefined(typeof(Frames), frameType) == false || reservedBits != 0)
            {
                return null;
            }

            // 计算数据长度和mask索引
            var maskIndex = 2;
            if (length == 127)
            {
                if (builder.Length < maskIndex + 8)
                {
                    return null;
                }
                length = (int)builder.ToUInt64(maskIndex);
                maskIndex = maskIndex + 8;
            }
            else if (length == 126)
            {
                if (builder.Length < maskIndex + 2)
                {
                    return null;
                }
                length = (int)builder.ToUInt16(maskIndex);
                maskIndex = maskIndex + 2;
            }

            // 检查数据长度
            if (builder.Length < maskIndex + Math.Max(4, length))
            {
                return null;
            }

            // 数据内容的索引位置
            var dataIndex = maskIndex + 4;
            if (length > 0)
            {
                fixed (byte* pdata = &builder.Source[dataIndex], pmask = &builder.Source[maskIndex])
                {
                    for (var i = 0; i < length; i++)
                    {
                        *(pdata + i) = (byte)(*(pdata + i) ^ *(pmask + i % 4));
                    }
                }
            }

            // 将数据放到resultBuilder
            contentBuilder.Add(builder.Source, dataIndex, length);
            // 清除已分析的数据
            builder.Remove(dataIndex + length);

            // 检查数据是否传输完成
            if (isFinal == true && frameType != Frames.Continuation)
            {
                var bytes = contentBuilder.ToArrayThenClear();
                return new FrameRequest { Frame = frameType, Content = bytes };
            }
            else
            {
                return null;
            }
        }
    }
}
