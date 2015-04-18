using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Json
{
    /// <summary>
    /// Api行为上下文
    /// </summary>
    [DebuggerDisplay("Action = {Action}")]
    public class ActionContext : RequestContext
    {
        /// <summary>
        /// 获取或设置Api行为对象
        /// </summary>
        public ApiAction Action { get; set; }

        /// <summary>
        /// Api行为上下文
        /// </summary>
        public ActionContext()
        {
        }

        /// <summary>
        /// Api行为上下文
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="action">Api行为</param>
        public ActionContext(RequestContext context, ApiAction action)
        {
            this.WebSocketServer = context.WebSocketServer;
            this.Client = context.Client;
            this.Packet = context.Packet;            
            this.Action = action;
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Action.ToString();
        }
    }
}
