using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Tasks
{
    /// <summary>
    /// 表示任务行为
    /// </summary>
    /// <typeparam name="TResult">任务结果类型</typeparam>
    public class TaskSetter<TResult> : ITaskSetter<TResult>, IDisposable
    {
        /// <summary>
        /// 任务源
        /// </summary>
        private readonly TaskCompletionSource<TResult> taskSource;

        /// <summary>
        /// 取消源
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 获取任务的返回值类型
        /// </summary>
        public Type ValueType
        {
            get
            {
                return typeof(TResult);
            }
        }


        /// <summary>
        /// 任务行为
        /// </summary>
        public TaskSetter()
        {
            this.taskSource = new TaskCompletionSource<TResult>();
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 设置任务的行为结果
        /// </summary>     
        /// <param name="value">数据值</param>   
        /// <returns></returns>
        public bool SetResult(object value)
        {
            this.cancellationTokenSource.Dispose();
            return this.taskSource.TrySetResult((TResult)value);
        }

        /// <summary>
        /// 设置设置为异常
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns></returns>
        public bool SetException(Exception ex)
        {
            this.cancellationTokenSource.Dispose();
            return this.taskSource.TrySetException(ex);
        }

        /// <summary>
        /// 获取同步结果
        /// </summary>
        /// <returns></returns>
        public TResult GetResult()
        {
            try
            {
                return this.GetTask().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <returns></returns>
        public Task<TResult> GetTask()
        {
            return this.taskSource.Task;
        }

        /// <summary>
        /// 设置超时时间
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public ITaskSetter<TResult> TimeoutAfter(TimeSpan timeout)
        {
            this.cancellationTokenSource.CancelAfter(timeout);
            return this;
        }


        /// <summary>
        /// 注册超时后的委托
        /// </summary>
        /// <param name="action">委托</param>
        /// <returns></returns>
        public ITaskSetter<TResult> AfterTimeout(Action action)
        {
            this.cancellationTokenSource.Token.Register(action);
            return this;
        }

        /// <summary>
        /// 注册超时后的委托
        /// </summary>
        /// <param name="action">委托</param>
        /// <returns></returns>
        public ITaskSetter<TResult> AfterTimeout(Action<ITaskSetter<TResult>> action)
        {
            this.cancellationTokenSource.Token.Register(() => action(this));
            return this;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.cancellationTokenSource.Dispose();
        }
    }
}
