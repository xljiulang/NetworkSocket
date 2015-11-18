using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 请求方式
    /// 目前只支持Get和Post两种
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
