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
    internal sealed class FreeSessionStack<T> : IDisposable where T : SessionBase
    {
        /// <summary>
        /// 集合
        /// </summary>
        private ConcurrentStack<T> stack = new ConcurrentStack<T>();

        /// <summary>
        /// 获取会话对象数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.stack.Count;
            }
        }

        /// <summary>
        /// 添加会话对象
        /// </summary>
        /// <param name="session">会话对象</param>
        public void Add(T session)
        {
            this.stack.Push(session);
        }

        /// <summary>
        /// 取出会话对象
        /// 如果取出失败则返回null
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            T session;
            if (this.stack.TryPop(out session))
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
            var sessions = this.stack.ToArray();
            foreach (var session in sessions)
            {
                IDisposable disposable = session;
                disposable.Dispose();
            }
        }
    }
}
