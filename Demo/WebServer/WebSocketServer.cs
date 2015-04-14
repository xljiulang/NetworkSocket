using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket;
using NetworkSocket;

namespace WebServer
{
    public class WebSocketServer : WebSocketServerBase
    {
        /// <summary>
        /// 客户端以送文本信息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="text"></param>
        protected override void OnText(IClient<SendPacket> client, string text)
        {
            this.SendText(client, text + "++");
            Console.WriteLine(text);
        }

        protected override void OnBinary(IClient<SendPacket> client, byte[] bytes)
        {
        }
    }
}
