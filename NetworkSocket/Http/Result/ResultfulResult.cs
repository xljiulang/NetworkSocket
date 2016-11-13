using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// Resultful结果
    /// </summary>
    public class RestfulResult : JsonResult
    {
        /// <summary>
        /// 内容
        /// </summary>
        /// <param name="data">内容</param>
        public RestfulResult(object data)
            : base(data)
        {
        }

        /// <summary>
        /// 序列化成xml文本
        /// </summary>
        /// <param name="data">内容</param>
        /// <returns></returns>
        protected virtual string SerializeXml(object data)
        {
            throw new NotImplementedException("不支持application/xml的返回类型");
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            var accept = context.Request.Headers["Accept"];
            if (accept != null && accept.IndexOf("application/xml", StringComparison.OrdinalIgnoreCase) > -1)
            {
                var gzip = context.Request.IsAcceptGZip();
                var xml = this.SerializeXml(this.Data);
                context.Response.ContentType = "application/xml";
                context.Response.WriteResponse(xml, gzip);
            }
            else
            {
                base.ExecuteResult(context);
            }
        }
    }
}
