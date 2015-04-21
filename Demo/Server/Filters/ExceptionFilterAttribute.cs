using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;

namespace Server.Filters
{
    /// <summary>
    /// 异常处理过滤器
    /// </summary>
    public class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
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
