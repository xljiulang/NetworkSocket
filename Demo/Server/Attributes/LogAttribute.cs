using NetworkSocket.Fast.Filters;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Attributes
{
    public class LogAttribute : FilterAttribute, IActionFilter
    {
        public ILog Loger { get; set; }

        private string message;

        public LogAttribute(string message)
        {
            this.Loger = new Server.Database.Loger();
            this.message = message;
        }
        public void OnExecuting(NetworkSocket.Fast.ActionContext actionContext)
        {
            var log = string.Format("Client:{0} Action:{1} Message:{2}", actionContext.Client, actionContext.Action, this.message);
            this.Loger.Write(log);
        }

        public void OnExecuted(NetworkSocket.Fast.ActionContext actionContext)
        {
        }
    }
}
