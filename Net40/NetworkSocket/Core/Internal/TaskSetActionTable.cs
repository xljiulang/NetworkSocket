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
    internal class TaskSetActionTable : IDisposable
    {
        /// <summary>
        /// 任务行为字典
        /// </summary>
        private readonly ConcurrentDictionary<long, ITaskSetAction> table;

        /// <summary>
        /// 获取或设置超时时间(毫秒)
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int TimeOut { get; set; }

        /// <summary>
        /// 任务行为表
        /// </summary>
        public TaskSetActionTable()
        {
            this.TimeOut = 30 * 1000;
            this.table = new ConcurrentDictionary<long, ITaskSetAction>();
            LoopWorker.AddWork(this.CheckTaskActionTimeout);
        }

        /// <summary>
        /// 检测任务行为的超时
        /// </summary>       
        private void CheckTaskActionTimeout()
        {
            if (this.table.Count == 0)
            {
                return;
            }

            foreach (var key in this.table.Keys)
            {
                if (this.ProcessTimeout(key) == false) break;
            }
        }

        /// <summary>
        /// 处理任务的超时情况
        /// 如果还没有超时返回false
        /// </summary>
        /// <param name="key">任务的key</param>
        private bool ProcessTimeout(long key)
        {
            var taskSetAction = default(ITaskSetAction);
            if (this.table.TryGetValue(key, out taskSetAction) == false)
            {
                return true;
            }

            // 还没有超时
            if (Environment.TickCount - taskSetAction.CreateTime < TimeOut)
            {
                return false;
            }

            // 已超时
            if (this.table.TryRemove(key, out taskSetAction))
            {
                taskSetAction.SetException(new TimeoutException());
            }
            return true;
        }

        /// <summary>
        /// 添加回调信息记录       
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="taskSetAction">设置行为</param>       
        /// <returns></returns>
        public void Add(long key, ITaskSetAction taskSetAction)
        {
            this.table.TryAdd(key, taskSetAction);
        }

        /// <summary>      
        /// 获取并移除与key匹配值
        /// 如果没有匹配项，返回null
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns></returns>
        public ITaskSetAction Take(long key)
        {
            ITaskSetAction taskSetAction;
            this.table.TryRemove(key, out taskSetAction);
            return taskSetAction;
        }

        /// <summary>
        /// 取出并移除全部的项
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITaskSetAction> TakeAll()
        {
            var values = this.table.ToArray().Select(item => item.Value);
            this.Clear();
            return values;
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            this.table.Clear();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            LoopWorker.RemoveWork(this.CheckTaskActionTimeout);
        }
    }
}
