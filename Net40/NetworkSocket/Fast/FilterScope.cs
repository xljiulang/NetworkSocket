using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 过滤器范围
    /// </summary>
    public enum FilterScope
    {
        /// <summary>
        /// 全局级过滤
        /// </summary>
        Global,

        /// <summary>
        /// 服务级过滤
        /// </summary>
        Service,

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
