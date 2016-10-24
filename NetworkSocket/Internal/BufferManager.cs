using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 提供设置SocketAsyncEventArgs缓冲区或申请缓冲区
    /// </summary>
    internal static class BufferManager
    {
        /// <summary>
        /// 获取SocketAsyncEventArgs的缓存区大小(8k)
        /// </summary>
        public static readonly int ItemSize = 8 * 1024;

        /// <summary>
        /// 获取每个缓冲区SocketAsyncEventArgs的数量(256) 
        /// </summary>
        public static readonly int ItemCount = 256;


        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly object syncRoot = new object();

        /// <summary>
        /// 缓冲区块列表
        /// </summary>
        private static readonly LinkedList<BufferBlock> linkedList = new LinkedList<BufferBlock>();

        /// <summary>
        /// 设置SocketAsyncEventArgs缓冲区
        /// </summary>
        static BufferManager()
        {
            BufferManager.CreateBufferBlock();
        }

        /// <summary>
        /// 添加一个缓冲块
        /// </summary>
        private static void CreateBufferBlock()
        {
            BufferManager.linkedList.AddLast(new BufferBlock(ItemSize, ItemCount));
        }

        /// <summary>
        /// 申请一个缓冲区
        /// </summary>        
        /// <returns></returns>
        public static ArraySegment<byte> GetBuffer()
        {
            lock (BufferManager.syncRoot)
            {
                ArraySegment<byte>? buffer;
                while ((buffer = BufferManager.linkedList.Last.Value.AllocBuffer()) == null)
                {
                    BufferManager.CreateBufferBlock();
                }
                return buffer.Value;
            }
        }

        /// <summary>
        /// 设置SocketAsyncEventArgs缓存区
        /// </summary>        
        /// <param name="arg">SocketAsyncEventArgs对象</param>
        public static void SetBuffer(SocketAsyncEventArgs arg)
        {
            var buffer = BufferManager.GetBuffer();
            arg.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
        }


        /// <summary>
        /// 表示缓冲区数据块
        /// </summary>
        private class BufferBlock
        {
            /// <summary>
            /// 缓冲区大小
            /// </summary>
            private readonly int itemSize;

            /// <summary>
            /// 数据内容
            /// </summary>
            private readonly byte[] buffer;

            /// <summary>
            /// 有效数据的位置
            /// </summary>
            private int position = 0;

            /// <summary>
            /// 缓冲区数据块
            /// </summary>
            /// <param name="itemSize">缓冲区大小</param>
            /// <param name="itemCount">缓冲区数量</param>
            public BufferBlock(int itemSize, int itemCount)
            {
                this.itemSize = itemSize;
                this.buffer = new byte[itemSize * itemCount];
            }

            /// <summary>
            /// 分配一个缓冲区
            /// 当内存块满时返回null
            /// </summary>
            /// <returns></returns>
            public ArraySegment<byte>? AllocBuffer()
            {
                if (this.position == this.buffer.Length)
                {
                    return null;
                }
                var byteRange = new ArraySegment<byte>(this.buffer, this.position, this.itemSize);
                this.position = this.position + this.itemSize;
                return byteRange;
            }
        }
    }
}
