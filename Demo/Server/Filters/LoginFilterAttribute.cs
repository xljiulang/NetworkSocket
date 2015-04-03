using NetworkSocket.Fast;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Filters
{
    /// <summary>
    /// 登录过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LoginFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(NetworkSocket.Fast.ActionContext actionContext)
        {
            bool valid = actionContext.Client.TagBag.Logined ?? false;
            if (valid == false)
            {
                throw new Exception("未登录就尝试请求其它服务");
            }
        }
    }
}
