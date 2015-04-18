using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;

namespace WebSocket
{
    public class FastServer : FastWebSocketServer
    {
        protected override bool OnHandshake(NetworkSocket.IClient<NetworkSocket.WebSocket.Response> client, NetworkSocket.WebSocket.HandshakeRequest request, out NetworkSocket.WebSocket.StatusCodes code)
        {
            return base.OnHandshake(client, request, out code);
        }

        protected override void OnConnect(NetworkSocket.IClient<NetworkSocket.WebSocket.Response> client)
        {
            base.OnConnect(client);
        }

        protected override void OnClose(NetworkSocket.IClient<NetworkSocket.WebSocket.Response> client, NetworkSocket.WebSocket.StatusCodes code)
        {
            base.OnClose(client, code);
        }

        protected override void OnDisconnect(NetworkSocket.IClient<NetworkSocket.WebSocket.Response> client)
        {
            base.OnDisconnect(client);
        }
    }
}
