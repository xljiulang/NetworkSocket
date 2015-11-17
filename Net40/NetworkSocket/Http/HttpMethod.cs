using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 请求方式
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
        /// <summary>
        /// Put
        /// </summary>
        PUT = 4,
        /// <summary>
        /// Delete
        /// </summary>
        DELETE = 8,

        /// <summary>
        /// 全部
        /// </summary>
        ALL = GET | POST | PUT | DELETE
    }
}
