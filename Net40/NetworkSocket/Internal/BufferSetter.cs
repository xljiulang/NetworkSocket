using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 提供设置SocketAsyncEventArgs缓冲区
    /// </summary>
    internal static class BufferSetter
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
        static BufferSetter()
        {
            BufferSetter.CreateBufferBlock();
        }

        /// <summary>
        /// 添加一个缓冲块
        /// </summary>
        private static void CreateBufferBlock()
        {
            BufferSetter.linkedList.AddLast(new BufferBlock(ItemSize, ItemCount));
        }

        /// <summary>
        /// 设置SocketAsyncEventArgs缓存区
        /// </summary>        
        /// <param name="arg">SocketAsyncEventArgs对象</param>
        /// <exception cref="OutOfMemoryException"></exception>
        public static void SetBuffer(SocketAsyncEventArgs arg)
        {
            lock (BufferSetter.syncRoot)
            {
                while (BufferSetter.linkedList.Last.Value.SetBuffer(arg) == false)
                {
                    BufferSetter.CreateBufferBlock();
                }
            }
        }

        /// <summary>
        /// 表示缓冲区数据块
        /// </summary>
        private class BufferBlock
        {
            /// <summary>
            /// SocketAsyncEventArgs对象缓冲区大小
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
            /// <param name="itemSize">SocketAsyncEventArgs缓冲区大小</param>
            /// <param name="itemCount">SocketAsyncEventArgs数量</param>
            public BufferBlock(int itemSize, int itemCount)
            {
                this.itemSize = itemSize;
                this.buffer = new byte[itemSize * itemCount];
            }

            /// <summary>
            /// 设置SocketAsyncEventArgs缓存区
            /// </summary>        
            /// <param name="arg">SocketAsyncEventArgs对象</param>
            /// <returns></returns>
            public bool SetBuffer(SocketAsyncEventArgs arg)
            {
                if (this.position == this.buffer.Length)
                {
                    return false;
                }
                arg.SetBuffer(this.buffer, this.position, this.itemSize);
                this.position = this.position + this.itemSize;
                return true;
            }
        }
    }
}
