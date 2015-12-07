using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http请求上下文
    /// </summary>   
    public class RequestContext 
    {
        /// <summary>
        /// 获取请求上下文对象
        /// </summary>
        public HttpRequest Request { get; private set; }

        /// <summary>
        /// 获取回复上下文对象
        /// </summary>
        public HttpResponse Response { get; private set; }

        /// <summary>
        /// 获取或设置结果
        /// </summary>
        public ActionResult Result { get; set; }

        /// <summary>
        /// 请求上下文
        /// </summary>
        /// <param name="request">请求上下文对象</param>
        /// <param name="response">回复上下文对象</param>        
        internal RequestContext(HttpRequest request, HttpResponse response)
        {
            this.Request = request;
            this.Response = response;
        }
    }
}
