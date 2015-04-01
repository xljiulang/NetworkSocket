using NetworkSocket.Fast.Filters;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Attributes
{
    public class LogAttribute : FilterAttribute, IActionFilter
    {
        public ILog Loger { get; set; }

        private string log;

        public LogAttribute(string log)
        {
            this.log = log;
        }

        public void OnExecuting(NetworkSocket.SocketAsync<NetworkSocket.Fast.FastPacket> client, NetworkSocket.Fast.FastPacket packet)
        {
            this.Loger.Log(string.Format("cmd:{0} log:{1}", packet.Command, this.log));
        }

        public void OnExecuted(NetworkSocket.SocketAsync<NetworkSocket.Fast.FastPacket> client, NetworkSocket.Fast.FastPacket packet)
        {
        }
    }
}
