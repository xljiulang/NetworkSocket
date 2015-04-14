using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// Tcp服务接口
    /// </summary>
    /// <typeparam name="T">协议类型</typeparam>
    public interface ITcpServer<T> : IDisposable where T : PacketBase
    {
        /// <summary>
        /// 获取所有连接的客户端对象   
        /// </summary>
        ClientCollection<T> AliveClients { get; }

        /// <summary>
        /// 开始启动监听       
        /// </summary>
        /// <param name="port">端口</param>
        void StartListen(int port);

        /// <summary>
        /// 开始启动监听
        /// </summary>
        /// <param name="localEndPoint">要监听的本地IP和端口</param>
        void StartListen(IPEndPoint localEndPoint);
    }
}
