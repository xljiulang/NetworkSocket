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
        /// 获取http的Api行为对象
        /// </summary>
        public HttpAction Action { get; private set; }

        /// <summary>
        /// 获取当前会话对象
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// 获取所有会话对象
        /// </summary>
        public ISessionManager AllSessions { get; private set; }

        /// <summary>
        /// 获取所有SSE会话对象
        /// </summary>
        public IEnumerable<HttpEventSession> EventSession
        {
            get
            {
                return this.AllSessions.FilterWrappers<HttpEventSession>();
            }
        }

        /// <summary>
        /// Api行为上下文
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <param name="action">Api行为</param>
        /// <param name="context">上下文</param>
        public ActionContext(RequestContext requestContext, HttpAction action, IContenxt context)
            : base(requestContext.Request, requestContext.Response)
        {
            this.Action = action;
            this.Session = context.Session;
            this.AllSessions = context.AllSessions;
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
