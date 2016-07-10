using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示只允许DELETE请求的http方法   
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpDeleteAttribute : HttpMethodFilterAttribute
    {
        /// <summary>
        /// 只允许DELETE请求的http方法
        /// </summary>
        public HttpDeleteAttribute()
            : base(HttpMethod.DELETE)
        {
        }
    }
}
