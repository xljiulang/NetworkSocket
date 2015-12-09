using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpServer.Filters
{
    /// <summary>
    /// 日志过滤器
    /// </summary>
    public class LogFilterAttribute : HttpFilterAttribute
    {
        private string message;

        public LogFilterAttribute(string message)
        {
            this.message = message;
        }

        protected override void OnExecuting(ActionContext filterContext)
        {
            Console.WriteLine(message);            
        }
    }
}
