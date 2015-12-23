using MixServer.GlobalFilters;
using NetworkSocket;
using NetworkSocket.Fast;
using NetworkSocket.Flex;
using NetworkSocket.Http;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixServer.AppStart
{
    public static partial class Config
    {
        public static void ConfigMiddleware(TcpListener listener)
        {
            listener.Use<FlexPolicyMiddleware>();
            listener.Use<HttpMiddleware>().GlobalFilters.Add(new HttpGlobalFilter());
            listener.Use<FastMiddleware>().GlobalFilters.Add(new FastGlobalFilter());
            listener.Use<JsonWebSocketMiddleware>().GlobalFilters.Add(new WebSockeGlobalFilter());
            listener.OnDisconnected += Listener_OnDisconnected;
        }

        /// <summary>
        /// 会话断开连接时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="context"></param>
        static void Listener_OnDisconnected(object sender, IContenxt context)
        {
            if (context.Session.IsProtocol("websocket") != true)
            {
                return;
            }

            var name = context.Session.Tag.TryGet<string>("name");
            if (name == null)
            {
                return;
            }

            var webSocketSessions = context
                .AllSessions
                .FilterWrappers<JsonWebSocketSession>();

            var members = webSocketSessions
                .Select(item => item.Tag.TryGet<string>("name"))
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
