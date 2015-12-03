using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 提供可限制并发任务数  
    /// </summary>
    public static class LimitTask
    {
        /// <summary>
        /// 提供一个以设置为可全局共用的可限制并发任务数的任务工厂
        /// </summary>
        public static TaskFactory Factory { get; set; }

        /// <summary>
        /// 创建一个可限制并发任务数的任务工厂
        /// </summary>
        /// <param name="maxTaskCount">最大并发任务数</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public static TaskFactory NewFactory(int maxTaskCount)
        {
            if (maxTaskCount < 1)
            {
                throw new ArgumentOutOfRangeException("maxTaskCount");
            }
            var scheduler = new LimitTaskScheduler(maxTaskCount);
            return new TaskFactory(scheduler);
        }

        /// <summary>
        /// 并发任务限制调度器
        /// 这是微软提供的源代码
        /// https://msdn.microsoft.com/en-us/library/ee789351.aspx
        /// </summary>       
        /// <summary> 
        /// </summary>
        private class LimitTaskScheduler : TaskScheduler
        {
            /// <summary>
            /// 当前线程是否处理任务
            /// </summary>
            [ThreadStatic]
            private static bool _currentThreadIsProcessingItems;

            /// <summary>
            /// 待执行的任务链表
            /// </summary>
            private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

            /// <summary>
            /// 最大并发数
            /// </summary>
            private readonly int _maxDegreeOfParallelism;

            /// <summary>
            /// 调试器是否在处理任务
            /// </summary>
            private int _delegatesQueuedOrRunning = 0;

            /// <summary>
            /// 并发限制任务调度器
            /// </summary>
            /// <param name="maxDegreeOfParallelism">最大并发数</param>
            public LimitTaskScheduler(int maxDegreeOfParallelism)
            {
                if (maxDegreeOfParallelism < 1)
                {
                    throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
                }
                this._maxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            /// <summary>
            /// 添加任务到队列中
            /// </summary>
            /// <param name="task"></param>
            protected sealed override void QueueTask(Task task)
            {
                lock (this._tasks)
                {
                    this._tasks.AddLast(task);
                    if (this._delegatesQueuedOrRunning < this._maxDegreeOfParallelism)
                    {
                        ++this._delegatesQueuedOrRunning;
                        this.NotifyThreadPoolOfPendingWork();
                    }
                }
            }

            /// <summary>
            /// 通知线程池有任务要执行
            /// </summary>
            private void NotifyThreadPoolOfPendingWork()
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    _currentThreadIsProcessingItems = true;
                    try
                    {
                        // Process all available items in the queue.
                        while (true)
                        {
                            Task item;
                            lock (this._tasks)
                            {
                                if (this._tasks.Count == 0)
                                {
                                    --this._delegatesQueuedOrRunning;
                                    break;
                                }

                                item = this._tasks.First.Value;
                                this._tasks.RemoveFirst();
                            }

                            base.TryExecuteTask(item);
                        }
                    }

                    finally
                    {
                        _currentThreadIsProcessingItems = false;
                    }
                }, null);
            }

            /// <summary>
            /// 确定是否可以在此调用中同步执行提供的 System.Threading.Tasks.Task，如果可以，将执行该任务 
            /// </summary>
            /// <param name="task"></param>
            /// <param name="taskWasPreviouslyQueued"></param>
            /// <returns></returns>
            protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (!_currentThreadIsProcessingItems)
                {
                    return false;
                }

                if (taskWasPreviouslyQueued)
                {
                    this.TryDequeue(task);
                }

                return base.TryExecuteTask(task);
            }

            /// <summary>
            /// 尝试将以前排队到此计划程序中的Task取消排队
            /// </summary>
            /// <param name="task">要取消排队的Task</param>
            /// <returns></returns>
            protected sealed override bool TryDequeue(Task task)
            {
                lock (_tasks)
                {
                    return _tasks.Remove(task);
                }
            }

            /// <summary>
            /// 指示能够支持的最大并发级别
            /// </summary>
            public sealed override int MaximumConcurrencyLevel
            {
                get
                {
                    return _maxDegreeOfParallelism;
                }
            }

            /// <summary>
            /// 生成当前排队到计划程序中等待执行的Task实例的枚举
            /// </summary>
            /// <returns></returns>
            protected sealed override IEnumerable<Task> GetScheduledTasks()
            {
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(_tasks, ref lockTaken);
                    if (lockTaken)
                    {
                        return _tasks.ToArray();
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(_tasks);
                    }
                }
            }
        }
    }
}

