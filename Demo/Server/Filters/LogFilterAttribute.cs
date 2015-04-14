using NetworkSocket.Fast;
using NetworkSocket.Fast.Context;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Filters
{
    /// <summary>
    /// 日志过滤器
    /// </summary>
    public class LogFilterAttribute : FilterAttribute, IActionFilter
    {
        public ILog Loger { get; set; }

        private string message;

        public LogFilterAttribute(string message)
        {
            this.message = message;
        }        

        public void OnExecuting(ServerActionContext filterContext)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), filterContext.Client, filterContext.Action, this.message);
            this.Loger.Write(log);
        }

        public void OnExecuted(ServerActionContext filterContext)
        {           
        }
    }
}
