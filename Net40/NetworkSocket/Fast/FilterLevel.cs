using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 过滤器级别
    /// </summary>
    internal enum FilterLevel
    {
        /// <summary>
        /// 权限级过滤
        /// </summary>
        Authorization,

        /// <summary>
        /// 类级过滤
        /// </summary>
        Class,

        /// <summary>
        /// 方法级过滤
        /// </summary>
        Method,
    }
}
