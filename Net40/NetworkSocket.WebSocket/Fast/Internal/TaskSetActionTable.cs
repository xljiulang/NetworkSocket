using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 任务行为表
    /// 自带超时检测功能
    /// </summary>
    internal class TaskSetActionTable : IDisposable
    {
        /// <summary>
        /// 超时时间
        /// </summary>       
        private int timeOut = 30 * 1000;

        /// <summary>
        /// 任务行为字典
        /// </summary>
        private readonly ConcurrentDictionary<long, ITaskSetAction> table = new ConcurrentDictionary<long, ITaskSetAction>();


        /// <summary>
        /// 获取或设置超时时间(毫秒)
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int TimeOut
        {
            get
            {
                return this.timeOut;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("TimeOut", "TimeOut的值必须大于0");
                }
                this.timeOut = value;
            }
        }

        /// <summary>
        /// 任务行为表
        /// </summary>
        public TaskSetActionTable()
        {
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
                if (this.ProcessIfTimeout(key) == false)
                {
                    // 遇到还没超时的对象就退出检测
                    break;
                }
            }
        }

        /// <summary>
        /// 如果超时了就处理超时并返回true
        /// 否则返回false
        /// </summary>
        /// <param name="key">值</param>
        private bool ProcessIfTimeout(long key)
        {
            ITaskSetAction taskSetAction;
            if (this.table.TryGetValue(key, out taskSetAction))
            {
                // 还没有超时
                if (Environment.TickCount - taskSetAction.CreateTime < TimeOut)
                {
                    return false;
                }
            }

            if (this.table.TryRemove(key, out taskSetAction))
            {
                taskSetAction.SetAction(SetTypes.SetTimeoutException, null);
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
