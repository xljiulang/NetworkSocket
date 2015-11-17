using NetworkSocket.Core;
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
    public class LogFilterAttribute : FilterAttribute, IActionFilter
    {
        private string message;

        public LogFilterAttribute(string message)
        {
            this.message = message;
        }

        public void OnExecuting(ActionContext filterContext)
        {
            Console.WriteLine(message);
            // filterContext.Result = new JsonResult(new { Result = "设置了Result就会中止Action的执行" });
        }

        public void OnExecuted(ActionContext filterContext)
        {

        }
    }
}
