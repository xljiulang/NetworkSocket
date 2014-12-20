using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Tasks
{
    /// <summary>
    /// 表示限制并发数的任务
    /// </summary>
    public class LimitTask
    {
        /// <summary>
        /// 获取或设置当前的限制并发任务实例
        /// </summary>
        public static LimitTask Current { get; set; }

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
        public LimitTask(int maxConcurrencyLevel)
        {
            this.MaxConcurrencyLevel = maxConcurrencyLevel;
            var lcts = new LimitTaskScheduler(maxConcurrencyLevel);
            this.taskFactory = new TaskFactory(lcts);
        }

        /// <summary>
        /// 运行并返回一个任务
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task Run(Action action)
        {
            return this.taskFactory.StartNew(action);
        }

        /// <summary>
        /// 运行并返回一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public Task<T> Run<T>(Func<T> func)
        {
            return this.taskFactory.StartNew(func);
        }
    }
}
