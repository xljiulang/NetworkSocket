using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 异常过滤器
    /// </summary>
    public interface IExceptionFilter : IFilter
    {
        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="filterContext">上下文</param>
        void OnException(ExceptionContext filterContext);
    }
}
