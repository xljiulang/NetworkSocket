using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 服务行为上下文
    /// </summary>
    public class ActionContext : RequestContext
    {
        /// <summary>
        /// 获取或设置服务行为对象
        /// </summary>
        public FastAction Action { get; set; }

        /// <summary>
        /// 服务行为上下文
        /// </summary>
        public ActionContext()
        {
        }

        /// <summary>
        /// 服务行为上下文
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="action">服务行为</param>
        public ActionContext(RequestContext context, FastAction action)
        {
            this.Client = context.Client;
            this.Packet = context.Packet;
            this.Action = action;
        }
    }
}
