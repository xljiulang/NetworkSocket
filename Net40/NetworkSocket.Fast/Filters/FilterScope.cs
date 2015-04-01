using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Filters
{
    /// <summary>
    /// 过滤器范围
    /// </summary>
    public enum FilterScope
    {
        /// <summary>
        /// 权限过滤
        /// </summary>
        Authorization,

        /// <summary>
        /// 类级方法过滤
        /// </summary>
        ActionClass,

        /// <summary>
        /// 成员组方法过滤
        /// </summary>
        ActionMethod,
    }
}
