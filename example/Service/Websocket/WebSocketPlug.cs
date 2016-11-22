using NetworkSocket;
using NetworkSocket.Plugs;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Websocket
{
    class WebSocketPlug : PlugBase
    {
        /// <summary>
        /// 会话断开后事件
        /// </summary>
        protected override void OnDisconnected(object sender, IContenxt context)
        {
            this.ProcessOfflineNotify(context);
        }

        /// <summary>
        /// 处理离线通知
        /// </summary>
        /// <param name="context">上下文</param>
        private void ProcessOfflineNotify(IContenxt context)
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
                item.InvokeApi("OnMemberChange", 0, name.Value, members);
            }
        }
    }
}
