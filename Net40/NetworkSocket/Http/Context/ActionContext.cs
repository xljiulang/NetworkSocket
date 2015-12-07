using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http协议的Api执行上下文
    /// </summary>
    [DebuggerDisplay("Action = {Action}")]
    public class ActionContext : RequestContext, IActionContext
    {
        /// <summary>
        /// 获取http行为对象
        /// </summary>
        ApiAction IActionContext.Action
        {
            get
            {
                return this.Action;
            }
        }

        /// <summary>
        /// 获取http行为对象
        /// </summary>
        public HttpAction Action { get; private set; }

        /// <summary>
        /// Api行为上下文
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="action">Api行为</param>
        public ActionContext(RequestContext context, HttpAction action)
            : base(context.Request, context.Response)
        {
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
