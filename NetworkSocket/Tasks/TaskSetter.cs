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
        /// 获取所创建的任务
        /// </summary>
        public Task<TResult> Task
        {
            get
            {
                return this.taskSource.Task;
            }
        }

        /// <summary>
        /// 获取是否已超时
        /// </summary>
        public bool IsTimeout
        {
            get
            {
                return this.cancellationTokenSource.IsCancellationRequested;
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
        /// 任务行为
        /// 超时后回调timeoutCallback
        /// </summary>
        /// <param name="timeoutCallback">超时回调</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TaskSetter(Action<ITaskSetter> timeoutCallback)
        {
            if (timeoutCallback == null)
            {
                throw new ArgumentNullException("timeoutCallback");
            }
            this.taskSource = new TaskCompletionSource<TResult>();
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationTokenSource.Token.Register((state) => timeoutCallback(state as ITaskSetter), this);
        }

        /// <summary>
        /// 在timeout触发超时
        /// 从此刻算起计算超时
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public TaskSetter<TResult> TimeoutAfter(TimeSpan timeout)
        {
            this.cancellationTokenSource.CancelAfter(timeout);
            return this;
        }

        /// <summary>
        /// 设置任务的行为结果
        /// </summary>     
        /// <param name="value">数据值</param>   
        /// <returns></returns>
        bool ITaskSetter.SetResult(object value)
        {
            return this.SetResult((TResult)value);
        }

        /// <summary>
        /// 设置任务的行为结果
        /// </summary>     
        /// <param name="value">数据值</param>   
        /// <returns></returns>
        public bool SetResult(TResult value)
        {
            this.cancellationTokenSource.Dispose();
            return this.taskSource.TrySetResult(value);
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
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.cancellationTokenSource.Dispose();
        }
    }
}
