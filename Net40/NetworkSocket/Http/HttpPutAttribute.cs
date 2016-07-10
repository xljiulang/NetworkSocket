using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示只允许PUT请求的http方法   
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpPutAttribute : HttpMethodFilterAttribute
    {
        /// <summary>
        /// 只允许PUT请求的http方法
        /// </summary>
        public HttpPutAttribute()
            : base(HttpMethod.PUT)
        {
        }
    }
}
