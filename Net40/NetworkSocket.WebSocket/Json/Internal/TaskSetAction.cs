using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Json
{
    /// <summary>
    /// 任务设置行为信息
    /// </summary>
    [DebuggerDisplay("InitTime = {InitTime}")]
    internal class TaskSetAction
    {
        /// <summary>
        /// 获取初始化时系统时间
        /// </summary>
        public int InitTime { get; private set; }

        /// <summary>
        /// 获取设置行为
        /// </summary>
        public Action<SetTypes, string> SetAction { get; private set; }

        /// <summary>
        /// 任务设置行为
        /// </summary>
        /// <param name="setAction">设置行为</param>
        public TaskSetAction(Action<SetTypes, string> setAction)
        {
            this.InitTime = Environment.TickCount;
            this.SetAction = setAction;
        }
    }
}
