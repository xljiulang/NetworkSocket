using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;
using WebSocket.Filters;

namespace WebSocket
{
    public class MyWebsocketServer : FastWebSocketServer
    {
        public MyWebsocketServer()
        {
            Console.Title = "FastWebSocketServer";
            this.GlobalFilter.Add(new ExceptionFilterAttribute());
            this.BindService(this.GetType().Assembly); // 绑定服务
            this.StartListen(8282);
            Console.WriteLine("FastWebSocketServer服务已启动，端口：" + this.LocalEndPoint.Port);
            Console.WriteLine(@"请在浏览器打开Htmls\WebSocke.Html");
            CpuCounterHelper.CpuTimeChanged += CpuCounter_CpuTimeChanged;
        }

        /// <summary>
        /// Cpu有变化时
        /// 通知有NotifyFlag为true的所有客户端
        /// </summary>
        /// <param name="value"></param>
        private void CpuCounter_CpuTimeChanged(int value)
        {
            foreach (var session in this.AllSessions)
            {
                if (session.TagData.TryGet<bool>("NotifyFlag", true))
                {
                    session.InvokeApi("CpuTimeChanged", new
                    {
                        time = DateTime.Now.ToString("HH:mm:ss"),
                        value
                    });
                }
            }
        }

        /// <summary>
        /// 连接进来
        /// </summary>
        /// <param name="session"></param>
        protected override void OnConnect(FastWebSocketSession session)
        {
            Console.Title = "FastWebSocketServer 连接数：" + this.AllSessions.Count();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="session"></param>
        protected override void OnDisconnect(FastWebSocketSession session)
        {
            Console.Title = "FastWebSocketServer 连接数：" + this.AllSessions.Count();
        }
    }
}
