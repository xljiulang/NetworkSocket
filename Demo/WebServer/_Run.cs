using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Json;

namespace WebServer
{
    class _Run
    {
        static JsonWebSocketServer jwebServer = new JsonWebSocketServer();

        static void Main(string[] args)
        {
            Console.Title = "WebSocket";
            jwebServer.BindService<SystemService>();
            jwebServer.StartListen(8282);

            Console.WriteLine("JsonWebServer服务已启动，端口：" + jwebServer.LocalEndPoint.Port);
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("按任务键调用客户端的Api ..");
                Console.ReadLine();

                var client = jwebServer.AliveClients.FirstOrDefault();
                if (client != null)
                {
                    // 调用客户端进行sum运算
                    try
                    {
                        var sum = jwebServer.InvokeApi<int>(client, "sum", 1, 2, 3).Result;
                        Console.WriteLine("InvokeApi(sum, 1, 2, 3) return {0}", sum);
                    }
                    catch (AggregateException ex)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("没有连接的客户端 ..");
                }
            }
        }
    }
}
