using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;

namespace WebSocket
{
    class _Run
    {
        static FastWebSocketServer fastServer = new FastWebSocketServer();

        static void Main(string[] args)
        {
            Console.Title = "WebSocketServer";

            GlobalFilters.Add(new Filters.ExceptionFilterAttribute());
            fastServer.BindService<SystemService>();
            fastServer.StartListen(8282);
            Console.WriteLine("JsonWebServer服务已启动，端口：" + fastServer.LocalEndPoint.Port);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("按任务键调用客户端的Api ..");
                Console.ReadLine();

                var client = fastServer.Clients.FirstOrDefault();
                if (client == null)
                {
                    Console.WriteLine("没有连接的客户端 ..");
                }
                else
                {
                    // 调用客户端进行sum运算
                    try
                    {
                        var sum = fastServer.InvokeApi<int>(client, "sum", 1, 2, 3).Result;
                        Console.WriteLine("InvokeApi(sum, 1, 2, 3) return {0}", sum);
                        fastServer.InvokeApi(client, "notify", "这是服务器发来的通知");
                    }
                    catch (AggregateException ex)
                    {
                        Console.WriteLine("调用远程api异常" + ex.InnerException.Message);
                    }
                }
            }
        }
    }
}
