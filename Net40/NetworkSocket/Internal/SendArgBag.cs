using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkSocket
{
    /// <summary>
    /// 用于发送的SocketAsyncEventArgs无序集合
    /// </summary>      
    internal static class SendArgBag
    {
        /// <summary>
        /// SocketAsyncEventArgs无序集合
        /// </summary>
        private static readonly ConcurrentBag<SocketAsyncEventArgs> concurrentBag = new ConcurrentBag<SocketAsyncEventArgs>();

        /// <summary>
        /// 所有初始化过的数量
        /// </summary>
        private static int totalInitCount = 0;

        /// <summary>
        /// 获取所有初始化过的数量
        /// </summary>
        public static int TotalInitCount
        {
            get
            {
                return SendArgBag.totalInitCount;
            }
        }

        /// <summary>
        /// 元素数量
        /// </summary>
        public static int Count
        {
            get
            {
                return concurrentBag.Count;
            }
        }

        /// <summary>
        /// 添加SocketAsync
        /// </summary>
        /// <param name="eventArg">SocketAsyncEventArgs对象</param>
        public static void Add(SocketAsyncEventArgs eventArg)
        {
            concurrentBag.Add(eventArg);
        }

        /// <summary>
        /// 取出SocketAsyncEventArgs
        /// 如果取出失败，则new新的SocketAsyncEventArgs并返回
        /// 当触发Completed事件后将自动回收
        /// </summary>
        /// <returns></returns>
        public static SocketAsyncEventArgs Take()
        {
            SocketAsyncEventArgs eventArg;
            if (concurrentBag.TryTake(out eventArg) == false)
            {
                Interlocked.Increment(ref SendArgBag.totalInitCount);
                eventArg = new SocketAsyncEventArgs();
                eventArg.Completed += (sender, e) => SendArgBag.Add(e);
            }
            return eventArg;
        }
    }
}
