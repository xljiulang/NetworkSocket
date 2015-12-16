using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 定义任务控制行为接口
    /// </summary>
    internal interface ITaskSetAction
    {
        /// <summary>
        /// 获取任务创建时间
        /// </summary>
        int CreateTime { get; }

        /// <summary>
        /// 获取任务的返回值类型
        /// </summary>
        Type ValueType { get; }

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
    internal class TaskSetAction<T> : ITaskSetAction
    {
        /// <summary>
        /// 任务源
        /// </summary>
        private TaskCompletionSource<T> taskSource;

        /// <summary>
        /// 获取创建时间
        /// </summary>
        public int CreateTime { get; private set; }

        /// <summary>
        /// 获取任务的返回结果类型
        /// </summary>
        public Type ValueType { get; private set; }

        /// <summary>
        /// 任务设置行为
        /// </summary>               
        /// <param name="taskSource">任务源</param>
        public TaskSetAction(TaskCompletionSource<T> taskSource)
        {
            this.taskSource = taskSource;
            this.CreateTime = Environment.TickCount;
            this.ValueType = typeof(T);
        }

        /// <summary>
        /// 设置任务结果
        /// </summary>
        /// <param name="value">数据值</param>
        public bool SetResult(object value)
        {
            return this.taskSource.TrySetResult((T)value);
        }

        /// <summary>
        /// 设置异常
        /// </summary>
        /// <param name="ex">异常</param>
        public bool SetException(Exception ex)
        {
            return this.taskSource.TrySetException(ex);
        }
    }
}
