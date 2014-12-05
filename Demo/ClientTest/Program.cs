using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket;
using System.Net;

namespace ClientTest
{
    class Program
    {
        static Client client;

        static void Main(string[] args)
        {
            client = new Client();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 2233));

            var content = "NetworkSocket.WebSocket";
            var packet = new PolicyPacket(Encoding.UTF8.GetBytes(content));
            client.Send(packet);

            Console.ReadLine();
        }
    }
}
