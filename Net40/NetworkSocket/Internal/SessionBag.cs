using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 无效的会话对象无序集合
    /// 线程安全类型
    /// </summary>
    /// <typeparam name="T">会话</typeparam>   
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class SessionBag<T> : IDisposable where T : SessionBase
    {
        /// <summary>
        /// 集合
        /// </summary>
        private ConcurrentBag<T> bag = new ConcurrentBag<T>();

        /// <summary>
        /// 获取会话对象数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.bag.Count;
            }
        }

        /// <summary>
        /// 添加会话对象
        /// </summary>
        /// <param name="session">会话对象</param>
        public void Add(T session)
        {
            this.bag.Add(session);
        }

        /// <summary>
        /// 取出会话对象
        /// 如果取出失败，则返回T的默认值
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            T session;
            if (this.bag.TryTake(out session))
            {
                return session;
            }
            return default(T);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            var sessions = this.bag.ToArray();
            foreach (var session in sessions)
            {
                IDisposable disposable = session;
                disposable.Dispose();
            }
        }
    }
}
