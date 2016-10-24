using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示只允许Post请求的http方法   
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpPostAttribute : HttpMethodFilterAttribute
    {
        /// <summary>
        /// 只允许Post请求的http方法
        /// </summary>
        public HttpPostAttribute()
            : base(HttpMethod.POST)
        {
        }
    }
}
