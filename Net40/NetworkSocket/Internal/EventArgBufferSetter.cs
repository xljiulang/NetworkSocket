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
    internal static class EventArgBufferSetter
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly object syncRoot = new object();

        /// <summary>
        /// 缓冲区块列表
        /// </summary>
        private static List<BufferBlock> bufferBlockList = new List<BufferBlock>();


        /// <summary>
        /// 获取SocketAsyncEventArgs的缓存区大小(8k)
        /// </summary>
        public const int ARG_BUFFER_SIZE = 8 * 1024;

        /// <summary>
        /// 获取接收或发送的缓冲区连续内存块大小(2M) 
        /// </summary>
        public const int BUFFER_BLOCK_SIZE = ARG_BUFFER_SIZE * 256;


        /// <summary>
        /// 获取缓冲区内存块的数量 
        /// </summary>
        public static int BufferBlockCount
        {
            get
            {
                return bufferBlockList.Count;
            }
        }

        /// <summary>
        /// 静态构造器
        /// </summary>
        static EventArgBufferSetter()
        {
            bufferBlockList.Add(new BufferBlock());
        }

        /// <summary>
        /// 设置SocketAsyncEventArgs缓存区
        /// </summary>        
        /// <param name="arg">SocketAsyncEventArgs对象</param>
        /// <exception cref="OutOfMemoryException"></exception>
        public static void SetBuffer(SocketAsyncEventArgs arg)
        {
            lock (syncRoot)
            {
                var lastBlock = bufferBlockList[BufferBlockCount - 1];
                if (lastBlock.CanSetBuffer == false)
                {
                    bufferBlockList.Add(new BufferBlock());
                    lastBlock = bufferBlockList[BufferBlockCount - 1];
                }
                lastBlock.SetBuffer(arg);
            }
        }


        /// <summary>
        /// 表示缓冲区数据块
        /// 非线程安全类型
        /// </summary>
        private class BufferBlock
        {
            /// <summary>
            /// 偏移量
            /// </summary>
            private int offset = 0;

            /// <summary>
            /// 数据内容
            /// </summary>
            private byte[] buffer = new byte[BUFFER_BLOCK_SIZE];


            /// <summary>
            /// 获取是否可以设置Buffer
            /// </summary>
            public bool CanSetBuffer
            {
                get
                {
                    return offset < BUFFER_BLOCK_SIZE;
                }
            }


            /// <summary>
            /// 设置SocketAsyncEventArgs缓存区
            /// </summary>        
            /// <param name="arg">SocketAsyncEventArgs对象</param>
            public void SetBuffer(SocketAsyncEventArgs arg)
            {
                arg.SetBuffer(this.buffer, this.offset, ARG_BUFFER_SIZE);
                this.offset = this.offset + ARG_BUFFER_SIZE;
            }
        }
    }
}
