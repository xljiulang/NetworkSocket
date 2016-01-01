using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示Tcp会话对象队列
    /// 线程安全类型
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class TcpSessionQueue : IDisposable
    {
        /// <summary>
        /// 队列
        /// </summary>
        private readonly ConcurrentQueue<TcpSessionBase> queue = new ConcurrentQueue<TcpSessionBase>();

        /// <summary>
        /// 获取会话对象数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }

        /// <summary>
        /// 添加会话对象
        /// </summary>
        /// <param name="session">会话对象</param>
        public void Add(TcpSessionBase session)
        {
            this.queue.Enqueue(session);
        }

        /// <summary>
        /// 取出会话对象
        /// 如果取出失败则返回null
        /// </summary>
        /// <returns></returns>
        public TcpSessionBase Take()
        {
            TcpSessionBase session;
            if (this.queue.TryDequeue(out session))
            {
                return session;
            }
            return default(TcpSessionBase);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            var sessions = this.queue.ToArray();
            foreach (var session in sessions)
            {
                session.Dispose();
            }
        }
    }
}
