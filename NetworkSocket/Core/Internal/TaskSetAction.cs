using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 定义任务控制行为接口
    /// </summary>
    internal interface ITaskSetAction
    {
        /// <summary>
        /// 获取任务的ID
        /// </summary>
        long Id { get; }

        /// <summary>
        /// 获取任务的返回值类型
        /// </summary>
        Type ValueType { get; }


        /// <summary>
        /// 设置超时委托
        /// </summary>
        Action<ITaskSetAction> OnTimeout { set; }

        /// <summary>
        /// 设置任务的行为结果
        /// </summary>     
        /// <param name="value">数据值</param>   
        /// <returns></returns>
        bool SetResult(object value);

        /// <summary>
        /// 设置设置为异常
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns></returns>
        bool SetException(Exception ex);
    }


    /// <summary>
    /// 表示任务设置行为信息
    /// </summary>
    [DebuggerDisplay("CreateTime = {CreateTime}")]
    internal class TaskSetAction<T> : ITaskSetAction, IDisposable
    {
        /// <summary>
        /// 定时器
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// 任务源
        /// </summary>
        private readonly TaskCompletionSource<T> taskSource;


        /// <summary>
        /// 获取任务id
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// 获取任务的返回结果类型
        /// </summary>
        public Type ValueType { get; private set; }

        /// <summary>
        /// 获取或设置超时委托
        /// </summary>
        public Action<ITaskSetAction> OnTimeout { private get; set; }

        /// <summary>
        /// 任务设置行为
        /// </summary>               
        /// <param name="taskSource">任务源</param>
        /// <param name="id">任务id</param>
        /// <param name="timeout">超时时间 毫秒</param>
        public TaskSetAction(TaskCompletionSource<T> taskSource, long id, TimeSpan timeout)
        {
            this.taskSource = taskSource;
            this.Id = id;
            this.ValueType = typeof(T);
            this.timer = new Timer(this.TimerCallback, null, timeout, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// timer回调
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallback(object state)
        {
            this.timer.Dispose();
            if (this.OnTimeout != null)
            {
                this.OnTimeout(this);
            }
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
            this.timer.Dispose();
        }
    }
}
