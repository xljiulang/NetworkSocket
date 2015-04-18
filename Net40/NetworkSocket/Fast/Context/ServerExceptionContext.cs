using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Context
{
    /// <summary>
    /// 服务端异常上下文
    /// </summary>
    [DebuggerDisplay("Message = {Exception.Message}")]
    public class ServerExceptionContext : ExceptionContext
    {
        /// <summary>
        /// 获取或设置Tcp服务端实例
        /// </summary>
        public IFastTcpServer FastTcpServer { get; set; }

        /// <summary>
        /// 服务端异常上下文
        /// </summary>
        public ServerExceptionContext()
        {
        }

        /// <summary>
        /// 服务端异常上下文
        /// </summary>
        /// <param name="actionContext">Api行为上下文</param>
        /// <param name="exception">异常</param>
        public ServerExceptionContext(ServerActionContext actionContext, Exception exception)
            : base(actionContext, exception)
        {
            this.FastTcpServer = actionContext.FastTcpServer;
        }

        /// <summary>
        /// 异常上下文
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <param name="exception">异常</param>
        public ServerExceptionContext(ServerRequestContext requestContext, Exception exception)
            : base(requestContext, exception)
        {
            this.FastTcpServer = requestContext.FastTcpServer;
        }       
    }
}
