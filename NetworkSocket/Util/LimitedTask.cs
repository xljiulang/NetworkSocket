using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Util
{
    /// <summary>
    /// 提供可限制并发任务数  
    /// </summary>
    public static class LimitedTask
    {
        /// <summary>
        /// 提供限制同时工作任务数的任务工厂
        /// 默认MaxTaskCount为Environment.ProcessorCount * 5
        /// 可通过LimitTask.SetFactoryMaxTaskCount来调整数量
        /// </summary>
        public static readonly TaskFactory Factory = LimitedTask.NewFactory(Environment.ProcessorCount * 5);

        /// <summary>
        /// 设置LimitTask.Factory的最大并发数
        /// </summary>
        /// <param name="maxTaskCount">最大同事工作的任务数</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void SetFactoryMaxTaskCount(int maxTaskCount)
        {
            var scheduler = LimitedTask.Factory.Scheduler as LimitedTaskScheduler;
            scheduler.SetMaxTaskCount(maxTaskCount);
        }

        /// <summary>
        /// 创建一个可限制并发任务数的任务工厂
        /// </summary>
        /// <param name="maxTaskCount">最大并发任务数</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public static TaskFactory NewFactory(int maxTaskCount)
        {
            var scheduler = new LimitedTaskScheduler(maxTaskCount);
            return new TaskFactory(scheduler);
        }




        /// <summary>
        /// 并发任务限制调度器
        /// 这里参考了微软提供的源代码
        /// https://msdn.microsoft.com/en-us/library/ee789351.aspx
        /// </summary>       
        /// <summary> 
        /// </summary>
        private class LimitedTaskScheduler : TaskScheduler
        {
            /// <summary>
            /// 当前线程是否处理任务项
            /// </summary>
            [ThreadStatic]
            private static bool ThreadWorking;

            /// <summary>
            /// 允许的最大并发数
            /// </summary>
            private int maxTaskCount;

            /// <summary>
            /// 当前正在运行的任务数
            /// </summary>
            private int currentTaskCount = 0;

            /// <summary>
            /// 排队执行的任务
            /// </summary>
            private readonly ScheduledTasks scheduledTasks = new ScheduledTasks();

            /// <summary>
            /// 获取指示能够支持的最大并发级别
            /// </summary>
            public sealed override int MaximumConcurrencyLevel
            {
                get
                {
                    return this.maxTaskCount;
                }
            }

            /// <summary>
            /// 并发限制任务调度器
            /// </summary>
            /// <param name="maxTaskCount">最大并发数</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public LimitedTaskScheduler(int maxTaskCount)
            {
                this.SetMaxTaskCount(maxTaskCount);
            }

            /// <summary>
            /// 设置最大并发数
            /// </summary>
            /// <param name="maxTaskCount">最大并发数</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public void SetMaxTaskCount(int maxTaskCount)
            {
                if (maxTaskCount < 1)
                {
                    throw new ArgumentOutOfRangeException("maxTaskCount");
                }
                this.maxTaskCount = maxTaskCount;
            }

            /// <summary>
            /// 将Task排队到计划程序中
            /// </summary>
            /// <param name="task">任务</param>
            protected sealed override void QueueTask(Task task)
            {
                this.scheduledTasks.Add(task);

                // 是否要启动线程为抢任务
                if (this.currentTaskCount < this.maxTaskCount)
                {
                    Interlocked.Increment(ref this.currentTaskCount);
                    ThreadPool.UnsafeQueueUserWorkItem(this.ExecutePendingTasks, null);
                }
            }

            /// <summary>
            /// 在线程池中执行所有待执行的任务
            /// </summary>
            private void ExecutePendingTasks(object state)
            {
                ThreadWorking = true;
                try
                {
                    while (true)
                    {
                        var task = this.scheduledTasks.Take();
                        if (task == null)
                        {
                            break;
                        }
                        base.TryExecuteTask(task);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref this.currentTaskCount);
                    ThreadWorking = false;
                }
            }

            /// <summary>
            /// 尝试将以前排队到此计划程序中的Task取消排队
            /// </summary>
            /// <param name="task">要取消排队的Task</param>
            /// <returns>一个布尔值，该值指示是否已成功地将 task 参数取消排队</returns>
            protected sealed override bool TryDequeue(Task task)
            {
                return this.scheduledTasks.Remove(task);
            }


            /// <summary>
            /// 生成当前排队到计划程序中等待执行的Task实例的枚举
            /// </summary>
            /// <exception cref="NotSupportedException"></exception>
            /// <returns>一个允许遍历当前排队到此计划程序中的任务的枚举</returns>
            protected sealed override IEnumerable<Task> GetScheduledTasks()
            {
                return this.scheduledTasks.ToArray();
            }

            /// <summary>
            /// 确定是否可以在此调用中同步执行提供的Task，如果可以，将执行该任务 
            /// </summary>
            /// <param name="task">任务</param>
            /// <param name="taskWasPreviouslyQueued">指示任务之前是否已排队。如果此参数为 True，则该任务以前可能已排队（已计划）；如果为 False，则已知该任务尚未排队，此时将执行此调用，以便以内联方式执行该任务，而不用将其排队。</param>
            /// <returns>指示是否已以内联方式执行该任务</returns>
            protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (ThreadWorking == false)
                {
                    return false;
                }

                if (taskWasPreviouslyQueued == true)
                {
                    this.TryDequeue(task);
                }

                return base.TryExecuteTask(task);
            }
        }


        /// <summary>
        /// 表示排队中未执行的任务
        /// </summary>
        private class ScheduledTasks
        {
            /// <summary>
            /// 排队执行的任务链表
            /// </summary>
            private readonly LinkedList<Task> linkedList = new LinkedList<Task>();

            /// <summary>
            /// 获取同步锁
            /// </summary>
            public readonly object SyncRoot = new object();

            /// <summary>
            /// 添加一个任务到末尾
            /// </summary>
            /// <param name="task">任务</param>
            public void Add(Task task)
            {
                lock (this.SyncRoot)
                {
                    this.linkedList.AddLast(task);
                }
            }

            /// <summary>
            /// 从头部获取一个任务
            /// </summary>
            /// <returns></returns>
            public Task Take()
            {
                lock (this.SyncRoot)
                {
                    if (this.linkedList.Count == 0)
                    {
                        return null;
                    }

                    var task = this.linkedList.First.Value;
                    this.linkedList.RemoveFirst();
                    return task;
                }
            }

            /// <summary>
            /// 删除一个任务
            /// </summary>
            /// <param name="task">任务</param>
            public bool Remove(Task task)
            {
                lock (this.SyncRoot)
                {
                    return this.linkedList.Remove(task);
                }
            }

            /// <summary>
            /// 转换为数组
            /// </summary>
            /// <returns></returns>
            public Task[] ToArray()
            {
                lock (this.SyncRoot)
                {
                    return this.linkedList.ToArray();
                }
            }
        }
    }
}

