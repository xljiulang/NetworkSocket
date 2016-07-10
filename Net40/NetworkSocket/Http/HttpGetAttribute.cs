using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示只允许GET请求的http方法   
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpGetAttribute : HttpMethodFilterAttribute
    {
        /// <summary>
        /// 只允许GET请求的http方法
        /// </summary>
        public HttpGetAttribute()
            : base(HttpMethod.GET)
        {
        }
    }
}
