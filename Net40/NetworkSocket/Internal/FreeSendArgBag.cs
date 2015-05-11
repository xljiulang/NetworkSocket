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
    /// 空闲的用于发送的SocketAsyncEventArgs
    /// </summary>      
    [DebuggerDisplay("Count = {Count}")]
    internal static class FreeSendArgBag
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
                return FreeSendArgBag.totalInitCount;
            }
        }

        /// <summary>
        /// 获取当前空闲的元素数量
        /// </summary>
        public static int Count
        {
            get
            {
                return concurrentBag.Count;
            }
        }

        /// <summary>
        /// 添加SocketAsyncEventArgs
        /// </summary>
        /// <param name="arg">SocketAsyncEventArgs对象</param>
        public static void Add(SocketAsyncEventArgs arg)
        {
            concurrentBag.Add(arg);
        }

        /// <summary>
        /// 取出SocketAsyncEventArgs
        /// 如果取出失败，则new新的SocketAsyncEventArgs并返回
        /// 当触发Completed事件后将自动回收
        /// </summary>
        /// <returns></returns>
        public static SocketAsyncEventArgs Take()
        {
            SocketAsyncEventArgs arg;
            if (concurrentBag.TryTake(out arg) == false)
            {
                Interlocked.Increment(ref FreeSendArgBag.totalInitCount);
                arg = new SocketAsyncEventArgs();
                arg.Completed += (sender, e) => FreeSendArgBag.Add(e);
            }
            return arg;
        }
    }
}
