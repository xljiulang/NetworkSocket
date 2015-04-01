using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Filters
{
    /// <summary>
    /// 异常过滤器
    /// </summary>
    public interface IExceptionFilter : IFilter
    {
        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="exceptionContext">上下文</param>
        void OnException(ExceptionContext exceptionContext);
    }
}
