using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// SocketAsyncEventArgs无序集合
    /// 线程安全类型
    /// </summary>  
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class SocketAsyncEventArgPool
    {
        /// <summary>
        /// 获取惟一实例
        /// </summary>
        public static readonly SocketAsyncEventArgPool Instance = new SocketAsyncEventArgPool();

        /// <summary>
        /// 无序集合
        /// </summary>
        private ConcurrentBag<SocketAsyncEventArgs> bag = new ConcurrentBag<SocketAsyncEventArgs>();

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
        /// <param name="eventArg">SocketAsyncEventArgs对象</param>
        public void Add(SocketAsyncEventArgs eventArg)
        {
            this.bag.Add(eventArg);
        }

        /// <summary>
        /// 取出SocketAsyncEventArgs
        /// 如果取出失败，则new新的SocketAsyncEventArgs并返回
        /// 当触发Completed事件后将自动回收
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs Take()
        {
            SocketAsyncEventArgs eventArg;
            if (this.bag.TryTake(out eventArg) == false)
            {
                eventArg = new SocketAsyncEventArgs();
                eventArg.Completed += (sender, e) => Instance.Add(e);
            }
            return eventArg;
        }
    }
}
