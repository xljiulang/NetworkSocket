using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 异常上下文
    /// </summary>
    public class ExceptionContext : RequestContext
    {
        /// <summary>
        /// 获取或设置异常对象
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 异常上下文
        /// </summary>
        public ExceptionContext()
        {
        }

        /// <summary>
        /// 异常上下文
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="exception">异常</param>
        public ExceptionContext(ActionContext context, Exception exception)
        {
            this.Client = context.Client;
            this.Packet = context.Packet;
            this.Exception = exception;
        }

        /// <summary>
        /// 异常上下文
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="exception">异常</param>
        public ExceptionContext(RequestContext context, Exception exception)
        {
            this.Client = context.Client;
            this.Packet = context.Packet;
            this.Exception = exception;
        }
    }
}
