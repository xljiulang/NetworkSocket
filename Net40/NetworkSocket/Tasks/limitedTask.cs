using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Tasks
{
    /// <summary>
    /// 表示限制并发数的任务
    /// </summary>
    public class LimitedTask
    {
        /// <summary>
        /// 任务工厂
        /// </summary>
        private TaskFactory taskFactory;

        /// <summary>
        /// 获取最大并发数
        /// </summary>
        public int MaxConcurrencyLevel { get; private set; }

        /// <summary>
        /// 限制并发数的任务
        /// </summary>
        /// <param name="maxConcurrencyLevel">最大并发数</param>
        public LimitedTask(int maxConcurrencyLevel)
        {
            this.MaxConcurrencyLevel = maxConcurrencyLevel;
            var scheduler = new LimitedTaskScheduler(maxConcurrencyLevel);
            this.taskFactory = new TaskFactory(scheduler);
        }

        /// <summary>
        /// 运行并返回一个任务
        /// </summary>
        /// <param name="action">要异步执行的操作委托</param>
        /// <returns></returns>
        public Task Run(Action action)
        {
            return this.taskFactory.StartNew(action);
        }

        /// <summary>
        /// 运行并返回一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">一个函数委托，可返回能够通过Task{TResult}获得的将来结果</param>
        /// <returns></returns>
        public Task<T> Run<T>(Func<T> function)
        {
            return this.taskFactory.StartNew(function);
        }


        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <param name="action">要异步执行的操作委托</param>
        /// <param name="state">一个包含由 action 委托使用的数据的对象</param>
        /// <returns></returns>
        public Task Run(Action<object> action, object state)
        {
            return this.taskFactory.StartNew(action, state);
        }

        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <param name="action">要异步执行的操作委托</param>
        /// <param name="cancellationToken">将指派给新任务的CancellationToken</param>
        /// <returns></returns>
        public Task Run(Action action, CancellationToken cancellationToken)
        {
            return this.taskFactory.StartNew(action, cancellationToken);
        }

        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <param name="action">要异步执行的操作委托</param>
        /// <param name="creationOptions">一个 TaskCreationOptions 值</param>
        /// <returns></returns>
        public Task Run(Action action, TaskCreationOptions creationOptions)
        {
            return this.taskFactory.StartNew(action, creationOptions);
        }


        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">一个函数委托，可返回能够通过Task{TResult}获得的将来结果</param>
        /// <param name="state">一个包含由 function 委托使用的数据的对象</param>
        /// <returns></returns>
        public Task<T> Run<T>(Func<object, T> function, object state)
        {
            return this.taskFactory.StartNew(function, state);
        }

        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">一个函数委托，可返回能够通过Task{TResult}获得的将来结果</param>
        /// <param name="cancellationToken">将指派给新任务的CancellationToken</param>
        /// <returns></returns>
        public Task<T> Run<T>(Func<T> function, CancellationToken cancellationToken)
        {
            return this.taskFactory.StartNew(function, cancellationToken);
        }


        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">一个函数委托，可返回能够通过Task{TResult}获得的将来结果</param>
        /// <param name="creationOptions">一个 TaskCreationOptions 值，用于控制创建的任务的行为</param>
        /// <returns></returns>
        public Task<T> Run<T>(Func<T> function, TaskCreationOptions creationOptions)
        {
            return this.taskFactory.StartNew(function, creationOptions);
        }

        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <param name="action">要异步执行的操作委托</param>
        /// <param name="state">一个包含由 function 委托使用的数据的对象</param>
        /// <param name="cancellationToken">将指派给新任务的CancellationToken</param>
        /// <returns></returns>
        public Task Run(Action<object> action, object state, CancellationToken cancellationToken)
        {
            return this.taskFactory.StartNew(action, state, cancellationToken);
        }

        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <param name="action">要异步执行的操作委托</param>
        /// <param name="state">一个包含由 function 委托使用的数据的对象</param>
        /// <param name="creationOptions">一个 TaskCreationOptions 值，用于控制创建的任务的行为</param>
        /// <returns></returns>
        public Task Run(Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            return this.taskFactory.StartNew(action, state, creationOptions);
        }


        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">一个函数委托，可返回能够通过Task{TResult}获得的将来结果</param>
        /// <param name="state">一个包含由 function 委托使用的数据的对象</param>
        /// <param name="cancellationToken">将指派给新任务的CancellationToken</param>
        /// <returns></returns>
        public Task<T> Run<T>(Func<object, T> function, object state, CancellationToken cancellationToken)
        {
            return this.taskFactory.StartNew(function, state, cancellationToken);
        }

        /// <summary>
        /// 创建并启动一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">一个函数委托，可返回能够通过Task{TResult}获得的将来结果</param>
        /// <param name="state">一个包含由 function 委托使用的数据的对象</param>
        /// <param name="creationOptions">一个 TaskCreationOptions 值，用于控制创建的任务的行为</param>
        /// <returns></returns>
        public Task<T> Run<T>(Func<object, T> function, object state, TaskCreationOptions creationOptions)
        {
            return this.taskFactory.StartNew(function, state, creationOptions);
        }
    }
}
