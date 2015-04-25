using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetworkSocket
{
    /// <summary>
    /// 表示会话额外信息
    /// </summary>
    public sealed class SessionExtraState
    {
        /// <summary>
        /// 发送字节数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private long sendByteCount = 0;
        /// <summary>
        /// 接收字节数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private long recvByteCount = 0;
        /// <summary>
        /// 发送总次数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private long sendTimes = 0;
        /// <summary>
        /// 接收总次数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private long recvTimes = 0;


        /// <summary>
        /// 获取连接成功的时间
        /// </summary>
        public DateTime ConnectedTime { get; private set; }

        /// <summary>
        /// 获取最近一次发送数据的时间
        /// </summary>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// 获取最近一次发送数据的时间
        /// </summary>
        public DateTime LastRecvTime { get; private set; }


        /// <summary>
        /// 获取最大发送的数据包字节数
        /// </summary>
        public int MaxSendSize { get; private set; }
        /// <summary>
        /// 获取最大接收的数据包字节数
        /// </summary>
        public int MaxRecvSize { get; private set; }
        /// <summary>
        /// 获取最小发送的数据包字节数
        /// </summary>
        public int MinSendSize { get; private set; }
        /// <summary>
        /// 获取最小接收的数据包字节数
        /// </summary>
        public int MinRecvSize { get; private set; }

        /// <summary>
        /// 获取发送总次数
        /// </summary>
        public long SendTimes
        {
            get
            {
                return this.sendTimes;
            }
        }

        /// <summary>
        /// 获取接收总次数
        /// </summary>
        public long RecvTimes
        {
            get
            {
                return this.recvTimes;
            }
        }

        /// <summary>
        /// 获取发送字节总数
        /// </summary>
        public long TotalSendByteCount
        {
            get
            {
                return this.sendByteCount;
            }
        }

        /// <summary>
        /// 获取接收字节总数
        /// </summary>
        public long TotalRecvByteCount
        {
            get
            {
                return this.recvByteCount;
            }
        }

        /// <summary>
        /// 表示额外信息
        /// </summary>
        internal SessionExtraState()
        {
        }

        /// <summary>
        /// 绑定时更新信息
        /// </summary>
        internal void SetBinded()
        {
            this.sendByteCount = 0;
            this.recvByteCount = 0;
            this.sendTimes = 0;
            this.recvTimes = 0;
            this.ConnectedTime = DateTime.Now;
        }

        /// <summary>
        /// 更新发送信息
        /// </summary>
        /// <param name="byteCount">发送的字节数</param>
        internal void SetSended(int byteCount)
        {
            Interlocked.Increment(ref this.sendTimes);
            Interlocked.Add(ref this.sendByteCount, byteCount);

            this.LastSendTime = DateTime.Now;

            if (this.MaxSendSize < byteCount)
            {
                this.MaxSendSize = byteCount;
            }

            if (this.MinSendSize == 0 || this.MinSendSize > byteCount)
            {
                this.MinSendSize = byteCount;
            }
        }


        /// <summary>
        /// 更新接收信息
        /// </summary>
        /// <param name="byteCount">接收的字节数</param>
        internal void SetRecved(int byteCount)
        {
            Interlocked.Increment(ref this.recvTimes);
            Interlocked.Add(ref this.recvByteCount, byteCount);

            this.LastRecvTime = DateTime.Now;

            if (this.MaxRecvSize < byteCount)
            {
                this.MaxRecvSize = byteCount;
            }

            if (this.MinRecvSize == 0 || this.MinRecvSize > byteCount)
            {
                this.MinRecvSize = byteCount;
            }
        }
    }
}
