using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示只允许Post请求或优先Post请求到的http方法
    /// 不可继承
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HttpPostAttribute : Attribute
    {
    }
}
