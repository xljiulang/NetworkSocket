using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示服务器额外信息
    /// </summary>
    public sealed class ServerExtraState
    {
        /// <summary>
        /// 获取空闲的会话对象数量
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<int> getFreeSessionCountFunc;

        /// <summary>
        /// 获取所有的会话数量
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<int> getTotalSessionCountFunc;

        /// <summary>
        /// 获取接受会话失败的次数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<int> getAcceptFailureTimesFunc { get; set; }

        /// <summary>
        /// 表示服务器额外信息
        /// </summary>
        /// <param name="getFreeSessionCountFunc">获取空闲的会话对象数量</param>
        /// <param name="getTotalSessionCountFunc">获取所有的会话数量</param>
        /// <param name="getAcceptFailureTimesFunc">获取接受会话失败的次数</param>
        internal ServerExtraState(
            Func<int> getFreeSessionCountFunc,
            Func<int> getTotalSessionCountFunc,
            Func<int> getAcceptFailureTimesFunc)
        {
            this.getFreeSessionCountFunc = getFreeSessionCountFunc;
            this.getTotalSessionCountFunc = getTotalSessionCountFunc;
            this.getAcceptFailureTimesFunc = getAcceptFailureTimesFunc;
        }

        /// <summary>
        /// 获取用于接收的SocketAsyncEventArgs对象的缓冲区大小
        /// </summary>
        public int RecvArgBufferSize
        {
            get
            {
                return EventArgBufferSetter.ARG_BUFFER_SIZE;
            }
        }

        /// <summary>
        /// 获取接收缓冲区连续内存块大小 
        /// </summary>
        public int RecvArgBufferBlockSize
        {
            get
            {
                return EventArgBufferSetter.BUFFER_BLOCK_SIZE;
            }
        }

        /// <summary>
        /// 获取接收缓冲区连续内存块的数量
        /// </summary>
        public int RecvArgBufferBlockCount
        {
            get
            {
                return EventArgBufferSetter.BufferBlockCount;
            }
        }    

        /// <summary>
        /// 获取已回收的会话对象的数量
        /// </summary>
        public int FreeSessionCount
        {
            get
            {
                return this.getFreeSessionCountFunc();
            }
        }

        /// <summary>
        /// 获取所有的会话对象数量
        /// 含FreeSession
        /// </summary>
        public int TotalSessionCount
        {
            get
            {
                return this.getTotalSessionCountFunc();
            }
        }

        /// <summary>
        /// 获取接受会话失败的次数
        /// </summary>
        public int AcceptFailureTimes
        {
            get
            {
                return this.getAcceptFailureTimesFunc();
            }
        }
    }
}
