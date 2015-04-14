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
    /// <typeparam name="T">发送数据包协议</typeparam>
    /// <typeparam name="TRecv">接收到的数据包类型</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class SocketClientBag<T, TRecv> where T : PacketBase
    {
        /// <summary>
        /// 无序集合
        /// </summary>
        private ConcurrentBag<SocketClient<T, TRecv>> bag = new ConcurrentBag<SocketClient<T, TRecv>>();

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
        public void Add(SocketClient<T, TRecv> client)
        {
            this.bag.Add(client);
        }

        /// <summary>
        /// 取出并返回客户端
        /// 如果取出失败，则new新的SocketClient并返回
        /// </summary>
        /// <returns></returns>
        public SocketClient<T, TRecv> Take()
        {
            SocketClient<T, TRecv> client;
            if (this.bag.TryTake(out client))
            {
                return client;
            }
            return new SocketClient<T, TRecv>();
        }
    }
}
