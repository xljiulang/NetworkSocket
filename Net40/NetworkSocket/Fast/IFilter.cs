using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 滤过器接口
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// 获取执行顺序
        /// 越小最优先
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 获取是否允许多个实例存在
        /// </summary>
        bool AllowMultiple { get; }
    }
}
