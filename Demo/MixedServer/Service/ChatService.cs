using MixedServer.Filter;
using NetworkSocket.Core;
using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixedServer.Service
{
    /// <summary>
    /// 在线聊天
    /// </summary>
    public class ChatService : FastApiService
    {
        /// <summary>
        /// 获取其它成员
        /// </summary>
        public IEnumerable<FastWebSocketSession> OtherSessions
        {
            get
            {
                foreach (var session in this.CurrentContext.AllSessions)
                {
                    if (session != this.CurrentContext.Session)
                    {
                        yield return session;
                    }
                }
            }
        }

        /// <summary>
        /// 获取成员列表
        /// </summary>
        /// <returns></returns>
        [Api]
        public string[] GetAllMembers()
        {
            var members = this.CurrentContext
                .AllSessions
                .Select(item => (string)item.TagBag.Name)
                .Where(item => item != null)
                .ToArray();

            return members;
        }

        /// <summary>
        /// 设置昵称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Api]
        public object SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new { state = false, message = "昵称不能为空 .." };
            }

            if (this.OtherSessions.Any(item => item.TagBag.Name == name))
            {
                return new { state = false, message = "此昵称已经被占用 .." };
            }

            // 推送成员上线
            this.CurrentContext.Session.TagBag.Name = name;
            foreach (var session in this.CurrentContext.AllSessions)
            {
                session.TryInvokeApi("OnMemberChange", 1, name);
            }
            return new { state = true, message = "设置昵称成功 .." };
        }

        /// <summary>
        /// 发送群聊内容
        /// </summary>
        /// <param name="message">内容</param>
        /// <returns></returns>        
        [Api]
        [SetNameFilter] //设置了昵称才可以发言
        public bool GroupMessage(string message)
        {
            if (string.IsNullOrEmpty(message) == true)
            {
                return false;
            }

            var name = (string)this.CurrentContext.Session.TagBag.Name; // 发言人
            foreach (var session in this.OtherSessions)
            {
                session.TryInvokeApi("OnGroupMessage", name, message, DateTime.Now.ToString("HH:mm:ss"));
            }
            return true;
        }
    }
}
