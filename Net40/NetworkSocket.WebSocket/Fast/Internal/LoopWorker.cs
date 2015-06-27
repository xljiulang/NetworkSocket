using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 由一个线程循环工作的对象
    /// 线程安全类型
    /// </summary>
    internal static class LoopWorker
    {
        /// <summary>
        /// 工作线程
        /// </summary>
        private readonly static Thread thread;

        /// <summary>
        /// 同步对象
        /// </summary>
        private readonly static object syncRoot = new object();

        /// <summary>
        /// 工作内容队列
        /// </summary>
        private readonly static List<Action> workList = new List<Action>();

        /// <summary>
        /// 获取当前的工作数量
        /// </summary>
        public static int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return workList.Count;
                }
            }
        }

        /// <summary>
        /// 表示构造器
        /// </summary>
        static LoopWorker()
        {
            thread = new Thread(new ThreadStart(LoopWork));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 循环工作
        /// </summary>
        private static void LoopWork()
        {
            var spinWait = new SpinWait();
            while (true)
            {
                WorkOnce();
                spinWait.SpinOnce();
            }
        }

        /// <summary>
        /// 一轮工作
        /// </summary>
        private static void WorkOnce()
        {
            lock (syncRoot)
            {
                workList.ForEach(work => work.Invoke());
            }
        }

        /// <summary>
        /// 添加工作
        /// </summary>
        /// <param name="work">工作</param>
        public static void AddWork(Action work)
        {
            lock (syncRoot)
            {
                workList.Add(work);
            }
        }

        /// <summary>
        /// 删除工作
        /// </summary>
        /// <param name="work">工作</param>
        public static void RemoveWork(Action work)
        {
            lock (syncRoot)
            {
                workList.Remove(work);
            }
        }
    }
}
