using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http错误结果
    /// </summary>
    public class ErrorResult : ActionResult
    {
        /// <summary>
        /// 获取或设置状态码
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 获取或设置错误内容
        /// </summary>
        public string Errors { get; set; }


        /// <summary>
        /// Http错误结果
        /// </summary>
        public ErrorResult()
        {
        }

        /// <summary>
        /// Http错误结果
        /// </summary>
        /// <param name="ex">http异常</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorResult(HttpException ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException();
            }
            this.Status = ex.Status;
            this.Errors = ex.Message;
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            var gzip = context.Request.IsAcceptGZip();
            context.Response.Status = this.Status;
            context.Response.StatusDescription = ((HttpStatusCode)this.Status).ToString();
            context.Response.WriteResponse(this.Errors, gzip);
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="response">回复对象</param>
        public void ExecuteResult(HttpResponse response)
        {
            response.Status = this.Status;
            response.StatusDescription = ((HttpStatusCode)this.Status).ToString();
            response.WriteResponse(this.Errors);
        }
    }
}
