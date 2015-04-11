using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 任务设置行为
    /// </summary>
    internal enum SetTypes
    {
        /// <summary>
        /// 设置结果
        /// </summary>
        SetReult,

        /// <summary>
        /// 设置异常
        /// </summary>
        SetException,

        /// <summary>
        /// 设置超时
        /// </summary>
        SetTimeout
    }
}
