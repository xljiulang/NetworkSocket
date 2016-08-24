using NetworkSocket.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示任务行为管理表
    /// 线程安全类型
    /// </summary>
    [DebuggerDisplay("Count = {table.Count}")]
    internal class TaskSetActionTable
    {
        /// <summary>
        /// 任务行为字典
        /// </summary>
        private readonly ConcurrentDictionary<long, ITaskSetAction> table;

        /// <summary>
        /// 任务行为表
        /// </summary>
        public TaskSetActionTable()
        {
            this.table = new ConcurrentDictionary<long, ITaskSetAction>();
        }

        /// <summary>
        /// 创建任务记录并添加到列表中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">任务id</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public Task<T> Create<T>(long id, TimeSpan timeout)
        {
            var taskSetAction = new TaskSetAction<T>(this, id, timeout);
            this.table.TryAdd(id, taskSetAction);
            return taskSetAction.Task;
        }

        /// <summary>      
        /// 获取并移除与id匹配的任务
        /// 如果没有匹配则返回null
        /// </summary>
        /// <param name="id">任务id</param>
        /// <returns></returns>
        public ITaskSetAction Take(long id)
        {
            ITaskSetAction taskSetAction;
            this.table.TryRemove(id, out taskSetAction);
            return taskSetAction;
        }

        /// <summary>
        /// 取出并移除全部的任务
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITaskSetAction> TakeAll()
        {
            var values = this.table.Values.ToArray();
            this.table.Clear();
            return values;
        }

        /// <summary>
        /// 清除所有任务
        /// </summary>
        public void Clear()
        {
            this.table.Clear();
        }


        /// <summary>
        /// 表示任务设置行为信息
        /// </summary>
        [DebuggerDisplay("Id = {Id}")]
        private class TaskSetAction<T> : ITaskSetAction, IDisposable
        {
            /// <summary>
            /// 任务列表
            /// </summary>
            private TaskSetActionTable table;


            /// <summary>
            /// 定时器
            /// </summary>
            private readonly Timer timer;

            /// <summary>
            /// 任务源
            /// </summary>
            private readonly TaskCompletionSource<T> taskSource;

            /// <summary>
            /// 获取任务的id
            /// </summary>
            public long Id { get; private set; }

            /// <summary>
            /// 获取任务的返回结果类型
            /// </summary>
            public Type ValueType { get; private set; }

            /// <summary>
            /// 获取任务
            /// </summary>
            public Task<T> Task
            {
                get
                {
                    return this.taskSource.Task;
                }
            }

            /// <summary>
            /// 任务设置行为
            /// </summary>               
            /// <param name="table">任务列表</param>
            /// <param name="id">任务id</param>
            /// <param name="timeout">超时时间</param>
            public TaskSetAction(TaskSetActionTable table, long id, TimeSpan timeout)
            {
                this.Id = id;
                this.ValueType = typeof(T);

                this.table = table;
                this.taskSource = new TaskCompletionSource<T>();
                this.timer = new Timer(this.OnTimerCallback, null, timeout, TimeSpan.FromMilliseconds(-1));
            }

            /// <summary>
            /// timer回调
            /// </summary>
            /// <param name="state"></param>
            private void OnTimerCallback(object state)
            {
                this.table.Take(this.Id);
                this.timer.Dispose();
                this.SetException(new TimeoutException());
                this.table = null;
            }

            /// <summary>
            /// 设置任务结果
            /// </summary>
            /// <param name="value">数据值</param>
            public bool SetResult(object value)
            {
                this.timer.Dispose();
                return this.taskSource.TrySetResult((T)value);
            }

            /// <summary>
            /// 设置异常
            /// </summary>
            /// <param name="ex">异常</param>
            public bool SetException(Exception ex)
            {
                this.timer.Dispose();
                return this.taskSource.TrySetException(ex);
            }

            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                this.table = null;
                this.timer.Dispose();
            }
        }
    }
}
