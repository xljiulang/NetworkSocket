using FastModels;
using FastServer.Services;
using NetworkSocket;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastServer
{
    /// <summary>
    /// MyFastServer服务 
    /// </summary>
    public class FastServer : FastTcpServer
    {
        /// <summary>
        /// 接收到会话连接
        /// </summary>
        /// <param name="session">会话</param>
        protected override void OnConnect(FastSession session)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), session, "Connect", "ConnectCount(" + this.AllSessions.Count() + ")");
            Console.WriteLine(log);
        }

        /// <summary>
        /// 接收到会话断开连接
        /// </summary>
        /// <param name="session">会话</param>
        protected override void OnDisconnect(FastSession session)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), session, "Disconnect", "ConnectCount(" + this.AllSessions.Count() + ")");
            Console.WriteLine(log);
        }
    }
}
