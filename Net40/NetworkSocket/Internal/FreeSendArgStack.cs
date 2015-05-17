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
    internal static class FreeSendArgStack
    {
        /// <summary>
        /// 所有初始化过的数量
        /// </summary>
        private static int initedCount = 0;

        /// <summary>
        /// SocketAsyncEventArgs集合
        /// </summary>
        private static readonly ConcurrentStack<SocketAsyncEventArgs> stack = new ConcurrentStack<SocketAsyncEventArgs>();


        /// <summary>
        /// 获取所有初始化过的数量
        /// </summary>
        public static int InitedCount
        {
            get
            {
                return initedCount;
            }
        }

        /// <summary>
        /// 获取当前空闲的元素数量
        /// </summary>
        public static int Count
        {
            get
            {
                return stack.Count;
            }
        }

        /// <summary>
        /// 添加SocketAsyncEventArgs
        /// </summary>
        /// <param name="arg">SocketAsyncEventArgs对象</param>
        public static void Add(SocketAsyncEventArgs arg)
        {
            stack.Push(arg);
        }

        /// <summary>
        /// 取出SocketAsyncEventArgs
        /// 如果取出失败，则new新的SocketAsyncEventArgs并返回
        /// 当触发Completed事件后将自动回收
        /// </summary>
        /// <returns></returns>
        public static SocketAsyncEventArgs TakeOrCreate()
        {
            SocketAsyncEventArgs arg;
            if (stack.TryPop(out arg) == false)
            {
                Interlocked.Increment(ref FreeSendArgStack.initedCount);
                arg = new SocketAsyncEventArgs();
                arg.Completed += (sender, e) => FreeSendArgStack.Add(e);
            }
            return arg;
        }
    }
}
