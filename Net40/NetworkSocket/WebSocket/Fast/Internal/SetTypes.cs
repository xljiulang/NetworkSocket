using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 任务设置行为
    /// </summary>
    internal enum SetTypes
    {
        /// <summary>
        /// 设置远程返回结果
        /// </summary>
        SetReturnReult,

        /// <summary>
        /// 设置远程返回的异常
        /// </summary>
        SetReturnException,

        /// <summary>
        /// 设置超时引起异常
        /// </summary>
        SetTimeoutException,

        /// <summary>
        /// 设置远程端关闭连接异常
        /// </summary>
        SetShutdownException,
    }
}
