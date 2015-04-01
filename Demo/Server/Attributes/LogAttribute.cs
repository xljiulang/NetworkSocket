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

        private string log;

        public LogAttribute(string log)
        {
            this.log = log;
        }

        public void OnExecuting(NetworkSocket.Fast.ActionContext actionContext)
        {
            this.Loger.Log(string.Format("cmd:{0} log:{1}", actionContext.Action.Command, this.log));
        }

        public void OnExecuted(NetworkSocket.Fast.ActionContext actionContext)
        {
        }
    }
}
