using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;
using System.Diagnostics;
using System.Threading;
using NetworkSocket;

namespace WebSocket
{
    class Program
    {
        static void Main(string[] args)
        {                      
            var server = new MyWebsocketServer();
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
