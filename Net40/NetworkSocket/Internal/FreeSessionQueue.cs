using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 空闲的会话对象集合
    /// 线程安全类型
    /// </summary>
    /// <typeparam name="T">会话</typeparam>   
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class FreeSessionQueue<T> : IDisposable where T : SessionBase
    {
        /// <summary>
        /// 集合
        /// </summary>
        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

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
        public void Add(T session)
        {
            this.queue.Enqueue(session);
        }

        /// <summary>
        /// 取出会话对象
        /// 如果取出失败则返回null
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            T session;
            if (this.queue.TryDequeue(out session))
            {
                return session;
            }
            return null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            var sessions = this.queue.ToArray();
            foreach (var session in sessions)
            {
                IDisposable disposable = session;
                disposable.Dispose();
            }
        }
    }
}
