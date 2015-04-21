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
        static FastServer fastServer = new FastServer();

        static void Main(string[] args)
        {
            Console.Title = "FastWebSocketServer";

            GlobalFilters.Add(new Filters.ExceptionFilterAttribute());
            fastServer.BindService<SystemService>();
            fastServer.StartListen(8282);
            Console.WriteLine("FastWebSocketServer服务已启动，端口：" + fastServer.LocalEndPoint.Port);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("按任意键调用客户端的Api ..");
                Console.ReadLine();

                foreach (var session in fastServer.AllSessions)
                {
                    try
                    {
                        var sum = session.InvokeApi<int>("sum", 1, 2, 3).Result;
                        session.InvokeApi("notify", "这是服务器发来的通知");

                        Console.WriteLine("{0} InvokeApi(sum, 1, 2, 3) return {1}", session, sum);
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
