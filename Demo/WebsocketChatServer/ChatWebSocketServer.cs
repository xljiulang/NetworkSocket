using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebsocketChatServer
{
    /// <summary>
    /// WebSocket聊天服务器
    /// </summary>
    public class ChatWebSocketServer : JsonWebSocketServer
    {
        /// <summary>
        /// WebSocket聊天服务器
        /// </summary>
        public ChatWebSocketServer()
        {
            this.GlobalFilters.Add(new ExceptionFilter()); // 异常处理
            this.BindService<ChatApiService>(); // 关联服务
            this.JsonSerializer = new JsonNetSerializer(); // 替换序列化工具
        }

        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="session"></param>
        protected override void OnDisconnect(JsonWebSocketSession session)
        {
            var name = (string)session.TagBag.Name;
            if (name == null)
            {
                return;
            }

            var members = this.AllSessions
               .Select(item => (string)item.TagBag.Name)
               .Where(item => item != null)
               .ToArray();

            // 推送成员下线通知
            foreach (var item in this.AllSessions)
            {
                item.TryInvokeApi("OnMemberChange", 0, name, members);
            }
        }
    }
}
