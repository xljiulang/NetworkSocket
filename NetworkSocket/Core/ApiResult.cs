using NetworkSocket.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示异步Api结果
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    public class ApiResult<TResult>
    {
        /// <summary>
        /// taskSetter
        /// </summary>
        private readonly TaskSetter<TResult> taskSetter;

        /// <summary>
        /// Api结果
        /// </summary>
        /// <param name="taskSetter">taskSetter</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ApiResult(TaskSetter<TResult> taskSetter)
        {
            if (taskSetter == null)
            {
                throw new ArgumentNullException();
            }
            this.taskSetter = taskSetter;
        }

        /// <summary>
        /// 在timeout触发超时
        /// 从此刻算起计算超时
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public ApiResult<TResult> TimeoutAfter(TimeSpan timeout)
        {
            this.taskSetter.TimeoutAfter(timeout);
            return this;
        } 

        /// <summary>
        /// 返回Awaiter对象
        /// </summary>
        /// <returns></returns>
        public TaskAwaiter<TResult> GetAwaiter()
        {
            return this.taskSetter.Task.GetAwaiter();
        }

        /// <summary>
        /// 隐式转换为Task类型
        /// </summary>
        /// <param name="apiResult"></param>
        /// <returns></returns>
        public static implicit operator Task<TResult>(ApiResult<TResult> apiResult)
        {
            return apiResult.taskSetter.Task;
        }
    }
}
