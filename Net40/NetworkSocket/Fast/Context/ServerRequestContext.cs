using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Context
{
    /// <summary>
    /// 服务端请求上下文
    /// </summary>
    [DebuggerDisplay("Packet = {Packet}")]
    public class ServerRequestContext : RequestContext
    {
        /// <summary>
        /// 获取或设置Tcp服务端实例
        /// </summary>
        public IFastTcpServer FastTcpServer { get; set; }
    }
}
