using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;
using WebSocket.Filters;
using System.Diagnostics;

namespace WebSocket
{
    /// <summary>
    /// Websocket服务器
    /// </summary>
    public class MyWebsocketServer : FastWebSocketServer
    {
        public MyWebsocketServer()
        {
            CpuCounterHelper.OnCpuTimeChanged += OnCpuTimeChanged;
        }

        /// <summary>
        /// Cpu有变化时
        /// 通知有NotifyFlag为true的所有客户端
        /// </summary>
        /// <param name="value"></param>
        private void OnCpuTimeChanged(int value)
        {
            var model = new
            {
                time = DateTime.Now.ToString("HH:mm:ss"),
                value
            };

            foreach (var session in this.AllSessions)
            {
                if (session.TagData.TryGet<bool>("NotifyFlag", true))
                {
                    session.InvokeApi("CpuTimeChanged", model);
                }
            }
        }

        /// <summary>
        /// 连接进来
        /// </summary>
        /// <param name="session"></param>
        protected override void OnConnect(FastWebSocketSession session)
        {
            Console.WriteLine("连接数：" + this.AllSessions.Count());
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="session"></param>
        protected override void OnDisconnect(FastWebSocketSession session)
        {
            Console.WriteLine("连接数：" + this.AllSessions.Count());
        }
    }
}
