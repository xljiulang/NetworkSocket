using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class _Run
    {
        static WebSocket13 webSocket = new WebSocket13();

        static void Main(string[] args)
        {
            webSocket.StartListen(8181);
            Console.Title = "WebServer";
            Console.WriteLine("服务已启动，端口：" + webSocket.LocalEndPoint.Port);

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
