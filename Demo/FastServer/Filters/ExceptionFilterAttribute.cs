using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;
using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using FastServer.Interfaces;

namespace FastServer.Filters
{
    /// <summary>
    /// 异常处理过滤器
    /// </summary>        
    public class ExceptionFilterAttribute : FastFilterAttribute
    {
        public IUserDao UserDao { get; set; }

        protected override void OnException(ExceptionContext filterContext)
        {           
            if (filterContext.Exception is ApiExecuteException)
            {
            }
            else if (filterContext.Exception is RemoteException)
            {
            }
            filterContext.ExceptionHandled = true;
        }
    }
}
