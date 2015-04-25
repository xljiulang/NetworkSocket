using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// SocketAsyncEventArgs缓冲区
    /// </summary>
    internal static class RecvArgBuffer
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// 缓冲区块
        /// </summary>
        private static List<byte[]> bufferBlockList = new List<byte[]>();

        /// <summary>
        /// 获取BufferBlock的Item的缓存区大小(8k)
        /// </summary>
        public static readonly int ItemBufferSize = 8 * 1024;

        /// <summary>
        /// 获取BufferBlock最大Item数(1024项)     
        /// </summary>
        public static readonly int ItemMaxCount = 1024;

        /// <summary>
        /// 获取当前BufferBlock的项数
        /// </summary>
        public static int CurrentBufferBlockItemCount { get; private set; }

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
        /// 设置缓存区
        /// </summary>        
        /// <param name="eventArg">参数</param>
        public static void SetBuffer(SocketAsyncEventArgs eventArg)
        {
            lock (RecvArgBuffer.syncRoot)
            {
                if (RecvArgBuffer.CurrentBufferBlockItemCount == RecvArgBuffer.ItemMaxCount)
                {
                    RecvArgBuffer.CurrentBufferBlockItemCount = 0;
                }

                if (RecvArgBuffer.CurrentBufferBlockItemCount == 0)
                {
                    var block = new byte[RecvArgBuffer.ItemBufferSize * RecvArgBuffer.ItemMaxCount];
                    RecvArgBuffer.bufferBlockList.Add(block);
                }

                var offset = RecvArgBuffer.CurrentBufferBlockItemCount * RecvArgBuffer.ItemBufferSize;
                eventArg.SetBuffer(RecvArgBuffer.bufferBlockList[RecvArgBuffer.bufferBlockList.Count - 1], offset, RecvArgBuffer.ItemBufferSize);
                RecvArgBuffer.CurrentBufferBlockItemCount++;
            }
        }
    }
}
