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
        private static List<byte[]> blockList = new List<byte[]>();

        /// <summary>
        /// 获取每项缓存区大小(8k)
        /// </summary>
        public static readonly int BlockItemBufferSize = 8 * 1024;

        /// <summary>
        /// 获取每块最大项数(1024项)     
        /// </summary>
        public static readonly int BlockItemMaxCount = 1024;

        /// <summary>
        /// 获取当前块的项数
        /// </summary>
        public static int BlockItemCount { get; private set; }


        /// <summary>
        /// 设置缓存区
        /// </summary>        
        /// <param name="eventArg">参数</param>
        public static void SetBuffer(SocketAsyncEventArgs eventArg)
        {
            lock (RecvArgBuffer.syncRoot)
            {
                if (RecvArgBuffer.BlockItemCount == RecvArgBuffer.BlockItemMaxCount)
                {
                    RecvArgBuffer.BlockItemCount = 0;
                }

                if (RecvArgBuffer.BlockItemCount == 0)
                {
                    var block = new byte[RecvArgBuffer.BlockItemBufferSize * RecvArgBuffer.BlockItemMaxCount];
                    RecvArgBuffer.blockList.Add(block);
                }

                var offset = RecvArgBuffer.BlockItemCount * RecvArgBuffer.BlockItemBufferSize;
                eventArg.SetBuffer(RecvArgBuffer.blockList[RecvArgBuffer.blockList.Count - 1], offset, RecvArgBuffer.BlockItemBufferSize);
                RecvArgBuffer.BlockItemCount++;
            }
        }
    }
}
