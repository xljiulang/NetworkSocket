using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 客户端无序集合
    /// 线程安全类型
    /// </summary>
    /// <typeparam name="T">PacketBase派生类型</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class SocketClientBag<T> where T : PacketBase
    {
        /// <summary>
        /// 无序集合
        /// </summary>
        private ConcurrentBag<SocketClient<T>> bag = new ConcurrentBag<SocketClient<T>>();

        /// <summary>
        /// 客户端数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.bag.Count;
            }
        }

        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="client">客户端对象</param>
        public void Add(SocketClient<T> client)
        {
            this.bag.Add(client);
        }

        /// <summary>
        /// 取出并返回客户端
        /// 如果取出失败，则new新的SocketClient并返回
        /// </summary>
        /// <returns></returns>
        public SocketClient<T> Take()
        {
            SocketClient<T> client;
            if (this.bag.TryTake(out client))
            {
                return client;
            }
            return new SocketClient<T>();
        }
    }
}
