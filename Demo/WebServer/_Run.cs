using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class _Run
    {
        static WebSocketServer webServer = new WebSocketServer();

        static void Main(string[] args)
        {
            webServer.StartListen(8181);
            Console.Title = "WebServer";
            Console.WriteLine("服务已启动，端口：" + webServer.LocalEndPoint.Port);

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
