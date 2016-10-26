using NetworkSocket.Core;
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
    public class RestfulResult : ActionResult
    {
        /// <summary>
        /// 内容
        /// </summary>
        protected object Data { get; private set; }

        /// <summary>
        /// 内容
        /// </summary>
        /// <param name="data">内容</param>
        public RestfulResult(object data)
        {
            this.Data = data;
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            // TODO
            // application/xml

            new JsonResult(this.Data).ExecuteResult(context);
        }
    }
}
