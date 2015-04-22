using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket.Filters
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
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), filterContext.Session, filterContext.Action, this.message);
            Console.WriteLine(log);
        }

        public void OnExecuted(ActionContext filterContext)
        {
        }
    }
}
