using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示请求方式
    /// 当前只支持Get和Post两种
    /// </summary>
    [Flags]
    public enum HttpMethod
    {
        /// <summary>
        /// Get
        /// </summary>
        GET = 1,
        /// <summary>
        /// Post
        /// </summary>
        POST = 2,
    }
}
