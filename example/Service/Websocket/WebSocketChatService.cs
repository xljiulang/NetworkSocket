using Models;
using NetworkSocket.Core;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Websocket
{
    /// <summary>
    /// jsonWebSocket在线聊天Api服务
    /// </summary>
    public sealed class WebSocketChatService : JsonWebSocketApiService
    {
        /// <summary>
        /// 获取其它成员
        /// </summary>
        public IEnumerable<JsonWebSocketSession> OtherSessions
        {
            get
            {
                return this
                    .CurrentContext
                    .JsonWebSocketSessions
                    .Where(item => item != this.CurrentContext.Session);
            }
        }

        /// <summary>
        /// 获取成员的名称
        /// </summary>
        /// <returns></returns>
        [Api]
        public string[] GetAllMembers()
        {
            var members = this
                .CurrentContext
                .JsonWebSocketSessions
                .Select(item => item.Tag.Get("name").AsString())
                .Where(item => item != null)
                .ToArray();

            return members;
        }

        /// <summary>
        /// 设置昵称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        [Api]
        public SetNameResult SetNickName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new SetNameResult { State = false, Message = "昵称不能为空 .." };
            }

            if (this.OtherSessions.Any(item => (string)item.Tag.Get("name").AsString() == name))
            {
                return new SetNameResult { State = false, Message = "此昵称已经被占用 .." };
            }

            // 推送新成员上线提醒
            this.CurrentContext.Session.Tag.Set("name", name);
            var members = this.GetAllMembers();

            foreach (var session in this.OtherSessions)
            {
                session.InvokeApi("OnMemberChange", 1, name, members);
            }
            return new SetNameResult { State = true, Name = name };
        }

        /// <summary>
        /// 成员发表群聊天内容
        /// </summary>
        /// <param name="message">内容</param>
        /// <returns></returns>        
        [Api]
        [WebSocketNickNameFilter] //设置了昵称才可以发言
        public bool ChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message) == true)
            {
                return false;
            }

            var name = (string)this.CurrentContext.Session.Tag.Get("name").AsString(); // 发言人
            foreach (var session in this.OtherSessions)
            {
                session.InvokeApi("OnChatMessage", name, message, DateTime.Now.ToString("HH:mm:ss"));
            }
            return true;
        }
    }
}
