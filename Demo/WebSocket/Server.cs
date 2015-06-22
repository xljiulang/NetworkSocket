using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;
using WebSocket.Filters;

namespace WebSocket
{
    public class Server : FastWebSocketServer
    {
        public Server()
        {
            Console.Title = "FastWebSocketServer";
            this.GlobalFilter.Add(new ExceptionFilterAttribute());
            this.BindService<CpuCounterService>();
            this.StartListen(8282);
            Console.WriteLine("FastWebSocketServer服务已启动，端口：" + this.LocalEndPoint.Port);

            CpuCounter.CpuTimeChanged += CpuCounter_CpuTimeChanged;
        }

        private void CpuCounter_CpuTimeChanged(int value)
        {
            foreach (var session in this.AllSessions)
            {
                if (session.TagData.TryGet<bool>("NotifyFlag", true))
                {
                    session.InvokeApi("CpuTimeChanged", new { time = DateTime.Now.ToString("HH:mm:ss"), value });
                }
            }
        }


        protected override void OnConnect(FastWebSocketSession session)
        {
            Console.Title = "FastWebSocketServer 连接数：" + this.AllSessions.Count();
        }

        protected override void OnDisconnect(FastWebSocketSession session)
        {
            Console.Title = "FastWebSocketServer 连接数：" + this.AllSessions.Count();
        }
    }
}
