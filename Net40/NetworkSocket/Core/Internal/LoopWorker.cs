using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 由一个线程循环工作的对象
    /// </summary>
    internal static class LoopWorker
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly object syncRoot = new object();

        /// <summary>
        /// 工作内容
        /// </summary>
        private static readonly List<Action> actions = new List<Action>();

        /// <summary>
        /// 表示构造器
        /// </summary>
        static LoopWorker()
        {
            Task.Factory.StartNew(() => LoopWork());
        }

        /// <summary>
        /// 循环工作
        /// </summary>
        private static void LoopWork()
        {
            var spinWait = new SpinWait();
            while (true)
            {
                lock (syncRoot)
                {
                    foreach (var item in actions)
                    {
                        item.Invoke();
                    }
                }
                spinWait.SpinOnce();
            }
        }

        /// <summary>
        /// 添加工作
        /// </summary>
        /// <param name="work">工作</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddWork(Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException();
            }
            lock (syncRoot)
            {
                actions.Add(work);
            }
        }

        /// <summary>
        /// 删除工作
        /// </summary>
        /// <param name="work">工作</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void RemoveWork(Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException();
            }
            lock (syncRoot)
            {
                actions.Remove(work);
            }
        }
    }
}
