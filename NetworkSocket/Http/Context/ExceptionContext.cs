using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http协议的Api异常上下文
    /// </summary>
    [DebuggerDisplay("Message = {Exception.Message}")]
    public class ExceptionContext : RequestContext, IExceptionContext
    {
        /// <summary>
        /// 获取异常对象
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// 获取或设置异常是否已处理
        /// 设置为true之后中止传递下一个Filter
        /// </summary>
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// 异常上下文
        /// </summary>
        /// <param name="actionContext">api行为上下文</param>
        /// <param name="exception">异常</param>
        public ExceptionContext(ActionContext actionContext, Exception exception)
            : base(actionContext.Request, actionContext.Response)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// 异常上下文
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <param name="exception">异常</param>
        public ExceptionContext(RequestContext requestContext, Exception exception)
            : base(requestContext.Request, requestContext.Response)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Exception.Message;
        }
    }
}
