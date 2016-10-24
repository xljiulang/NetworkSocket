using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示服务器发送事件结果
    /// 该结果将会话对象标记为SSE会话对象
    /// </summary>
    public class EventResult : ActionResult
    {
        /// <summary>
        /// 事件　
        /// </summary>
        private HttpEvent httpEvent;

        /// <summary>
        /// 服务器发送事件结果
        /// </summary>
        public EventResult()
        {
        }

        /// <summary>
        /// 服务器发送事件结果
        /// </summary>
        /// <param name="httpEvent">事件</param>
        public EventResult(HttpEvent httpEvent)
        {
            this.httpEvent = httpEvent;
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            context.Response.ContentType = "text/event-stream";
            context.Response.WriteHeader();

            var session = ((IWrapper)context.Response).UnWrap();
            var wapper = new HttpEventSession(session);
            session.SetProtocolWrapper(Protocol.SeverSendEvent, wapper);

            if (this.httpEvent != null)
            {
                wapper.SendEvent(this.httpEvent);
            }
        }
    }
}
