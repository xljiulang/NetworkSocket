using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示字节数组范围
    /// 不可继承
    /// </summary>
    public sealed class ByteRange
    {
        /// <summary>
        /// 获取偏移量
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// 获取字节数
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 获取字节数组
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// 表示字节数组范围
        /// </summary>
        /// <param name="buffer">字节数组</param>   
        /// <exception cref="ArgumentNullException"></exception>
        public ByteRange(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            this.Buffer = buffer;
            this.Count = buffer.Length;
            this.Offset = 0;
        }

        /// <summary>
        /// 表示字节数组范围
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">字节数</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ByteRange(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset", "offset值无效");
            }

            if (count < 0 || (offset + count) > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count", "count值无效");
            }
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
        }      
    }
}
