using NetworkSocket.Core;
using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixedServer.Filter
{
    /// <summary>
    /// 昵称设置过滤器
    /// </summary>
    public class SetNameFilter : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(ActionContext filterContext)
        {
            if (filterContext.Session.TagBag.Name == null)
            {
                // 未设置昵称的将抛出异常给客户端
                throw new Exception("请设置昵称后再聊天 ..");
            }
        }
    }
}
