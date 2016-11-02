using NetworkSocket.Core;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service.Fast
{
    /// <summary>
    /// 日志过滤器
    /// </summary>
    public class FastLogFilter : FastFilterAttribute
    {
        private string message;

        public FastLogFilter(string message)
        {
            this.message = message;
        }

        protected override void OnExecuting(ActionContext filterContext)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), filterContext.Session, filterContext.Action, this.message);
            Console.WriteLine(log);
        }
    }
}
