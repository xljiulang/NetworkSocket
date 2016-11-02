using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service.Websocket
{
    /// <summary>
    /// 昵称设置过滤器
    /// </summary>
    public class WebSocketNickNameFilter : JsonWebSocketFilterAttribute
    {
        public WebSocketNickNameFilter()
        {
            this.Order = -1;
        }

        protected override void OnExecuting(ActionContext filterContext)
        {
            if (filterContext.Session.Tag.Get("name").IsNull)
            {
                filterContext.Result = "请设置昵称后再聊天";
            }
        }
    }
}
