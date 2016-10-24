using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示只允许某些请求的http方法   
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpMethodFilterAttribute : Attribute
    {
        /// <summary>
        /// 获取允许的http方法
        /// </summary>
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// 只允许某些请求的http方法 
        /// </summary>
        /// <param name="method">http方法 </param>
        /// <exception cref="ArgumentException"></exception>
        public HttpMethodFilterAttribute(HttpMethod method)
        {
            if (Enum.IsDefined(typeof(HttpMethod), method) == false)
            {
                throw new ArgumentException("参数method的值未在枚举里定义");
            }
            this.Method = method;
        }
    }
}
