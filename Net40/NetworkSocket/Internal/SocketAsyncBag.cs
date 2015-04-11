using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// SocketAsyn无序集合
    /// 线程安全类型
    /// </summary>
    /// <typeparam name="T">PacketBase派生类型</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class SocketAsyncBag<T> where T : PacketBase
    {
        /// <summary>
        /// 无序集合
        /// </summary>
        private ConcurrentBag<SocketAsync<T>> bag = new ConcurrentBag<SocketAsync<T>>();

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.bag.Count;
            }
        }

        /// <summary>
        /// 添加SocketAsync
        /// </summary>
        /// <param name="SocketAsync">SocketAsync对象</param>
        public void Add(SocketAsync<T> SocketAsync)
        {
            this.bag.Add(SocketAsync);
        }

        /// <summary>
        /// 取出并返回SocketAsync
        /// 如果取出失败，则new新的SocketAsync并返回
        /// </summary>
        /// <returns></returns>
        public SocketAsync<T> Take()
        {
            SocketAsync<T> socketAsyc;
            if (this.bag.TryTake(out socketAsyc))
            {
                return socketAsyc;
            }
            return new SocketAsync<T>();
        }
    }
}
