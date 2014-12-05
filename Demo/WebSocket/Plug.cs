using NetworkSocket;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket
{
    public class Plug : PluginBase<Hybi13Packet>
    {
        public Plug(TcpServerBase<Hybi13Packet> context)
            : base(context)
        {
        }

        public override void OnConnect(NetworkSocket.SocketAsync<NetworkSocket.WebSocket.Hybi13Packet> client)
        {
            this.ServerContext.CloseClient(client);
        }

        public override void OnDisconnect(NetworkSocket.SocketAsync<NetworkSocket.WebSocket.Hybi13Packet> client)
        {

        }

        public override void OnRecvComplete(NetworkSocket.SocketAsync<NetworkSocket.WebSocket.Hybi13Packet> client, NetworkSocket.WebSocket.Hybi13Packet packet)
        {

        }

        public override void OnSend(NetworkSocket.SocketAsync<NetworkSocket.WebSocket.Hybi13Packet> client, NetworkSocket.WebSocket.Hybi13Packet packet)
        {

        }
    }
}
