using NetworkSocket;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Websocket
{
    class WebSocketPlug : IPlug
    {
        /// <summary>
        /// 会话连接后事件
        /// </summary>
        public void OnConnected(object sender, IContenxt context)
        {
        }

        public void OnSSLAuthenticated(object sender, IContenxt context, Exception exception)
        {
        }

        /// <summary>
        /// 会话断开后事件
        /// </summary>
        public void OnDisconnected(object sender, IContenxt context)
        {
            Console.WriteLine(context.Session .GetHashCode() + " OnDisconnected");
            this.ProcessOfflineNotify(context);
        }

        /// <summary>
        /// 服务异常事件
        /// </summary>
        public void OnException(object sender, Exception exception)
        {
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
