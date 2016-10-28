using Service.GlobalFilters;
using NetworkSocket;
using NetworkSocket.Flex;
using NetworkSocket.Http;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;

namespace Service.AppStart
{
    public static partial class Config
    {
        /// <summary>
        /// 配置中间件
        /// </summary>
        /// <param name="listener"></param>
        public static void ConfigMiddleware(TcpListener listener)
        {
            listener.Use<HttpMiddleware>().GlobalFilters.Add(new HttpGlobalFilter());
            listener.Use<JsonWebSocketMiddleware>().GlobalFilters.Add(new WebSockeGlobalFilter());
            listener.Use<FastMiddleware>().GlobalFilters.Add(new FastGlobalFilter());
            listener.Use<FlexPolicyMiddleware>();
            listener.Events.OnDisconnected += Events_OnDisconnected;
        }


        /// <summary>
        /// 会话断开连接时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="context"></param>
        static void Events_OnDisconnected(object sender, IContenxt context)
        {
            if (context.Session.Protocol != Protocol.WebSocket)
            {
                return;
            }

            var name = context.Session.Tag.Get("name");
            if (name.IsNull == true)
            {
                return;
            }

            var webSocketSessions = context
                .AllSessions
                .FilterWrappers<JsonWebSocketSession>();

            var members = webSocketSessions
                .Select(item => item.Tag.Get("name").AsString())
                .Where(item => item != null)
                .ToArray();

            // 推送成员下线通知
            foreach (var item in webSocketSessions)
            {
                item.InvokeApi("OnMemberChange", 0, name, members);
            }
        }
    }
}
