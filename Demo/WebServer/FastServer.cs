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
    }
}
