using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示字节数组范围
    /// 不可继承
    /// </summary>
    [DebuggerDisplay("Offset = {Offset}, Count = {Count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
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

        /// <summary>
        /// 分割为大小相等的ByteRange集合
        /// </summary>
        /// <param name="size">新的ByteRange大小</param>
        /// <returns></returns>
        public IEnumerable<ByteRange> SplitBySize(int size)
        {
            if (size >= this.Count)
            {
                yield return this;
                yield break;
            }

            var remain = this.Count % size;
            var count = this.Count - remain;

            var offset = 0;
            while (offset < count)
            {
                yield return new ByteRange(this.Buffer, this.Offset + offset, size);
                offset = offset + size;
            }

            if (remain > 0)
            {
                yield return new ByteRange(this.Buffer, offset, remain);
            }
        }

        /// <summary>
        /// 调试视图
        /// </summary>
        private class DebugView
        {
            /// <summary>
            /// 查看的对象
            /// </summary>
            private ByteRange view;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="view">查看的对象</param>
            public DebugView(ByteRange view)
            {
                this.view = view;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public byte[] Values
            {
                get
                {
                    var byteArray = new byte[this.view.Count];
                    System.Buffer.BlockCopy(this.view.Buffer, this.view.Offset, byteArray, 0, this.view.Count);
                    return byteArray;
                }
            }
        }
    }
}
