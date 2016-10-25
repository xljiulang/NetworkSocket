using NetworkSocket.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Tasks
{
    /// <summary>
    /// 表示任务管理表
    /// 线程安全类型
    /// </summary>
    /// <typeparam name="T">任务ID类型</typeparam>
    [DebuggerDisplay("Count = {table.Count}")]
    public class TaskSetterTable<T>
    {
        /// <summary>
        /// 任务行为字典
        /// </summary>
        private readonly ConcurrentDictionary<T, ITaskSetter> table;

        /// <summary>
        /// 任务行为表
        /// </summary>
        public TaskSetterTable()
        {
            this.table = new ConcurrentDictionary<T, ITaskSetter>();
        }

        /// <summary>
        /// 创建带id的任务并添加到列表中
        /// </summary>
        /// <typeparam name="TResult">任务结果类型</typeparam>
        /// <param name="id">任务id</param>
        /// <param name="timeout">任务超时时间，触发返回任务超时异常</param>
        /// <returns></returns>
        public Task<TResult> Create<TResult>(T id, TimeSpan timeout)
        {
            var taskSetter = new TaskSetter<T, TResult>(this, id, timeout);
            this.table.TryAdd(id, taskSetter);
            return taskSetter.Task;
        }

        /// <summary>      
        /// 获取并移除与id匹配的任务
        /// 如果没有匹配则返回null
        /// </summary>
        /// <param name="id">任务id</param>
        /// <returns></returns>
        public ITaskSetter Take(T id)
        {
            ITaskSetter taskSetter;
            this.table.TryRemove(id, out taskSetter);
            return taskSetter;
        }

        /// <summary>
        /// 取出并移除全部的任务
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITaskSetter> TakeAll()
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
        /// <typeparam name="TId">id类型</typeparam>
        /// <typeparam name="TResult">任务结果类型</typeparam>
        [DebuggerDisplay("Id = {Id}")]
        private class TaskSetter<TId, TResult> : ITaskSetter, IDisposable
        {
            /// <summary>
            /// 任务列表
            /// </summary>
            private TaskSetterTable<TId> table;

            /// <summary>
            /// 定时器
            /// </summary>
            private readonly Timer timer;

            /// <summary>
            /// 任务源
            /// </summary>
            private readonly TaskCompletionSource<TResult> taskSource;

            /// <summary>
            /// 获取任务的id
            /// </summary>
            public TId Id { get; private set; }

            /// <summary>
            /// 获取任务的返回结果类型
            /// </summary>
            public Type ValueType { get; private set; }

            /// <summary>
            /// 获取任务
            /// </summary>
            public Task<TResult> Task
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
            public TaskSetter(TaskSetterTable<TId> table, TId id, TimeSpan timeout)
            {
                this.Id = id;
                this.ValueType = typeof(TResult);

                this.table = table;
                this.taskSource = new TaskCompletionSource<TResult>();
                this.timer = new Timer(this.OnTimerCallback, null, timeout, TimeSpan.FromMilliseconds(-1));
            }

            /// <summary>
            /// timer回调
            /// </summary>
            /// <param name="state"></param>
            private void OnTimerCallback(object state)
            {
                this.table.Take(this.Id);
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
                return this.taskSource.TrySetResult((TResult)value);
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
