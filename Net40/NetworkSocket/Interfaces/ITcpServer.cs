using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkSocket.Interfaces
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
        SocketAsyncCollection<T> AliveClients { get; }

        /// <summary>
        /// 获取所监听的本地IP和端口
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// 获取服务是否已处在监听中
        /// </summary>
        bool IsListening { get; }



        /// <summary>
        /// 开始启动监听
        /// </summary>
        /// <param name="localEndPoint">要监听的本地IP和端口</param>
        void StartListen(IPEndPoint localEndPoint);

        /// <summary>
        /// 关闭并复用客户端对象
        /// </summary>
        /// <param name="client">客户端对象</param>
        bool CloseClient(SocketAsync<T> client);
    }
}
