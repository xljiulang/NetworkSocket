using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 定义结果值封装为task行为
    /// </summary>
    public interface ITaskWrapper
    {
        /// <summary>
        /// 获取值
        /// </summary>
        /// <returns></returns>
        object GetResult();

        /// <summary>
        /// 完成时继续延续任务
        /// </summary>
        /// <param name="action">行为</param>
        /// <returns></returns>
        ITaskWrapper ContinueWith(Action<ITaskWrapper> action);
    }
}
