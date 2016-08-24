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
    internal class TaskSetActionTable
    {
        /// <summary>
        /// 任务行为字典
        /// </summary>
        private readonly ConcurrentDictionary<long, ITaskSetAction> table;

        /// <summary>
        /// 任务行为表
        /// </summary>
        public TaskSetActionTable()
        {
            this.table = new ConcurrentDictionary<long, ITaskSetAction>();
        }

        /// <summary>
        /// 添加回调信息记录       
        /// </summary>        
        /// <param name="taskSetAction">设置行为</param>       
        /// <returns></returns>
        public void Add(ITaskSetAction taskSetAction)
        {
            taskSetAction.OnTimeout = this.OnTaskSetActionTimeOut;
            this.table.TryAdd(taskSetAction.Id, taskSetAction);
        }

        /// <summary>
        /// 任务超时处理
        /// </summary>
        /// <param name="taskSetAction">任务</param>
        private void OnTaskSetActionTimeOut(ITaskSetAction taskSetAction)
        {
            var storeTask = this.Take(taskSetAction.Id);
            if (storeTask != null)
            {
                storeTask.SetException(new TimeoutException());
            }
        }

        /// <summary>      
        /// 获取并移除与id匹配值
        /// 如果没有匹配项，返回null
        /// </summary>
        /// <param name="id">任务id</param>
        /// <returns></returns>
        public ITaskSetAction Take(long id)
        {
            ITaskSetAction taskSetAction;
            this.table.TryRemove(id, out taskSetAction);
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
    }
}
