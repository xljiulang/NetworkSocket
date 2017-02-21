using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NetworkSocket.Tasks
{

    /// <summary>
    /// 表示阻塞的任务行为
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    public class SyncTaskSetter<TResult> : ITaskSetter<TResult>, IDisposable
    {
        /// <summary>
        /// 结果值
        /// </summary>
        private TResult result;

        /// <summary>
        /// 通知事件
        /// </summary>
        private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);

        /// <summary>
        /// 获取值的类型
        /// </summary>
        public Type ValueType
        {
            get
            {
                return typeof(TResult);
            }
        }


        /// <summary>
        /// 设置结果
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        public bool SetResult(TResult value)
        {
            this.result = value;
            return this.resetEvent.Set();
        }

        /// <summary>
        /// 设置结果
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        bool ITaskSetter.SetResult(object value)
        {
            return this.SetResult((TResult)value);
        }

        /// <summary>
        /// 设置异常
        /// </summary>
        /// <param name="ex">异常</param>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        bool ITaskSetter.SetException(Exception ex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <returns></returns>
        public TResult GetResult()
        {
            this.resetEvent.WaitOne();
            return this.result;
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        public TResult GetResult(TimeSpan timeout)
        {
            if (this.resetEvent.WaitOne(timeout))
            {
                return this.result;
            }
            throw new TimeoutException();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.resetEvent.Dispose();
        }
    }
}
