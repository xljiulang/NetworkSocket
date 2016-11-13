using NetworkSocket.Core;
using System;

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
        protected object Data { get; private set; }

        /// <summary>
        /// application/json内容
        /// </summary>
        /// <param name="data">内容</param>
        public JsonResult(object data)
        {
            this.Data = data;
        }

        /// <summary>
        /// 序列化成json文本
        /// </summary>
        /// <param name="data">内容</param>
        /// <param name="datetimeFomat">时期时间格式</param>
        /// <returns></returns>
        protected virtual string SerializeJson(object data, string datetimeFomat)
        {
            return new DefaultDynamicJsonSerializer().Serialize(this.Data, datetimeFomat);
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            var callback = context.Request["callback"];
            var json = this.SerializeJson(this.Data, null);
            var gzip = context.Request.IsAcceptGZip();

            if (callback == null)
            {
                context.Response.ContentType = "application/json";
                context.Response.WriteResponse(json, gzip);
            }
            else
            {
                var jsonP = string.Format("{0}({1})", callback, json);
                context.Response.WriteResponse(jsonP, gzip);
            }
        }
    }
}
