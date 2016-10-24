using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 任务helper
    /// </summary>
    internal static class TaskHelper
    {
        /// <summary>
        /// 表示空的任务
        /// </summary>
        public static readonly Task Empty = Task.FromResult(0);
    }
}
