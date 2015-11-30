using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示application/json内容
    /// </summary>
    public class JsonResult : ActionResult
    {
        /// <summary>
        /// 内容
        /// </summary>
        private object data;

        /// <summary>
        /// application/json内容
        /// </summary>
        /// <param name="data">内容</param>
        public JsonResult(object data)
        {
            this.data = data;
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            var json = new JavaScriptSerializer().Serialize(data);
            context.Response.ContentType = "application/json";
            context.Response.Write(json);
        }
    }
}
