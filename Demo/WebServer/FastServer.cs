using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.WebSocket.Fast;

namespace WebSocket
{
    public class FastServer : FastWebSocketServer
    {
        protected override void OnText(FastWebSocketSession session, string content)
        {
            base.OnText(session, content);
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
