using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;
using System.Diagnostics;
using System.Threading;
using NetworkSocket;
using WebSocket.Services;
using WebSocket.Filters;

namespace WebSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "WebSocket";

            var server = new MyWebsocketServer();
            server.JsonSerializer = new JsonNetSerializer();
            server.GlobalFilter.Add(new ExceptionFilterAttribute());
            server.BindService<CpuCounterService>(); // 绑定服务
            server.StartListen(8282);

            Console.WriteLine("WebSocket服务已启动，端口：" + server.LocalEndPoint.Port);
            Process.Start(@"..\..\Htmls\WebSocket.html");

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
