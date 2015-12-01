using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixedServer
{
    /// <summary>
    /// WebSocket服务
    /// </summary>
    public class ChatWebSocketServer : FastWebSocketServer
    {
        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="session"></param>
        protected override void OnDisconnect(FastWebSocketSession session)
        {
            var name = (string)session.TagBag.Name;
            if (name == null)
            {
                return;
            }

            // 推送成员下线通知
            foreach (var item in this.AllSessions)
            {
                item.TryInvokeApi("OnMemberChange", 0, name);
            }
        }
    }
}
