using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// SocketAsyncEventArgs缓冲区管理
    /// </summary>
    internal class SocketAsyncEventArgBuffer
    {
        /// <summary>
        /// 获取惟一实例 
        /// </summary>
        public static readonly SocketAsyncEventArgBuffer Instance = new SocketAsyncEventArgBuffer();

        /// <summary>
        /// 同步锁
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// 缓冲区块
        /// </summary>
        private List<byte[]> blockList = new List<byte[]>();

        /// <summary>
        /// 获取每项缓存区大小(1024byte)
        /// </summary>
        public readonly int BlockItemBufferSize = 1024;

        /// <summary>
        /// 获取每块最大项数(1000项)     
        /// </summary>
        public readonly int BlockItemMaxCount = 1000;

        /// <summary>
        /// 获取当前块的项数
        /// </summary>
        public int BlockItemCount { get; private set; }


        /// <summary>
        /// 设置缓存区
        /// </summary>        
        /// <param name="eventArg">参数</param>
        public void SetBuffer(SocketAsyncEventArgs eventArg)
        {
            lock (this.syncRoot)
            {
                if (this.BlockItemCount == this.BlockItemMaxCount)
                {
                    this.BlockItemCount = 0;
                }

                if (this.BlockItemCount == 0)
                {
                    var block = new byte[this.BlockItemBufferSize * this.BlockItemMaxCount];
                    this.blockList.Add(block);
                }

                var offset = this.BlockItemCount * this.BlockItemBufferSize;
                eventArg.SetBuffer(this.blockList[blockList.Count - 1], offset, this.BlockItemBufferSize);
                this.BlockItemCount++;
            }
        }

        /// <summary>
        /// 清除所有数据块
        /// </summary>
        public void Clear()
        {
            this.blockList.Clear();
        }
    }
}
