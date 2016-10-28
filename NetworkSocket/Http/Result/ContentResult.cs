using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示text/html内容
    /// </summary>
    public class ContentResult : ActionResult
    {
        /// <summary>
        /// text/html内容
        /// </summary>
        protected string Content { get; private set; }

        /// <summary>
        /// text/html内容
        /// </summary>
        /// <param name="content">内容</param>
        public ContentResult(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            var gzip = context.Request.IsAcceptGZip();
            context.Response.WriteResponse(Content, gzip);
        }
    }
}
