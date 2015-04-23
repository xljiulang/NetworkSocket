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
    class _Run
    {
        static void Main(string[] args)
        {
            ByteBits b1 = 6;
            ByteBits b2 = 5;
            var list = new List<ByteBits> { b1, b2 };
            list.Sort();            
            var server = new Server();
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
