using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket;
using NetworkSocket;

namespace WebServer
{
    /// <summary>
    /// WebSocket服务
    /// </summary>
    public class WebSocketServer : WebSocketServerBase
    {
        /// <summary>
        /// 客户端以送文本信息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="text"></param>
        protected override void OnText(IClient<Response> client, string text)
        {
            text = string.Format("{0} => {1}", DateTime.Now.ToString("HH:mm:ss"), text);
            client.Send(text);
            Console.WriteLine(text);
        }

        protected override void OnClose(IClient<Response> client, CloseReasons reason)
        {
            base.OnClose(client, reason);
        }
    }
}
