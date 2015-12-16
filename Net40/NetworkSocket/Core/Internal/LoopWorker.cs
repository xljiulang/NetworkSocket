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
        /// 工作事件
        /// </summary>
        private static event Action WorkEvent;

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
                if (WorkEvent != null)
                {
                    WorkEvent.Invoke();
                }
                spinWait.SpinOnce();
            }
        }

        /// <summary>
        /// 添加工作
        /// </summary>
        /// <param name="work">工作</param>
        public static void AddWork(Action work)
        {
            WorkEvent += work;
        }

        /// <summary>
        /// 删除工作
        /// </summary>
        /// <param name="work">工作</param>
        public static void RemoveWork(Action work)
        {
            WorkEvent -= work;
        }
    }
}
