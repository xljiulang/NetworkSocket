using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Context
{
    /// <summary>
    /// 服务端Api行为上下文
    /// </summary>
    [DebuggerDisplay("Action = {Action}")]
    public class ServerActionContext : ActionContext
    {
        /// <summary>
        /// 获取或设置Tcp服务端实例
        /// </summary>
        public IFastTcpServer FastTcpServer { get; set; }

        /// <summary>
        /// 服务端Api行为上下文
        /// </summary>
        public ServerActionContext()
        {
        }

        /// <summary>
        ///  服务端Api行为上下文
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <param name="action">Api行为</param>
        public ServerActionContext(ServerRequestContext requestContext, ApiAction action)
            : base(requestContext, action)
        {
            this.FastTcpServer = requestContext.FastTcpServer;
        }
    }
}
