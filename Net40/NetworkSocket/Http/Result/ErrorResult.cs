using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http异常结果
    /// </summary>
    public class ErrorResult : ActionResult
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 错误内容
        /// </summary>
        public string Errors { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            context.Response.Status = this.Status;
            context.Response.Write(this.Errors);
        }
    }
}
