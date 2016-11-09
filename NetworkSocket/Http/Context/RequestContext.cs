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
        /// 获取http请求上下文对象
        /// </summary>
        public HttpRequest Request { get; private set; }

        /// <summary>
        /// 获取http回复上下文对象
        /// </summary>
        public HttpResponse Response { get; private set; }

        /// <summary>
        /// 获取或设置行为结果
        /// 当设置了值之后，执行停止并将结果执行
        /// </summary>
        public ActionResult Result { get; set; }

        /// <summary>
        /// 请求上下文
        /// </summary>        
        /// <param name="request">请求对象</param>
        /// <param name="response">回复对象</param>        
        public RequestContext(HttpRequest request, HttpResponse response)
        {
            this.Request = request;
            this.Response = response;
        }
    }
}
