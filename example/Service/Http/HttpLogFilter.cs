using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service.Http
{
    /// <summary>
    /// 日志过滤器
    /// </summary>
    public class HttpLogFilter : HttpFilterAttribute
    {
        private string message;

        public HttpLogFilter(string message)
        {
            this.message = message;
        }

        protected override void OnExecuting(ActionContext filterContext)
        {
            Console.WriteLine(message);            
        }
    }
}
