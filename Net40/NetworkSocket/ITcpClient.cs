using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// Tcp客户端接口
    /// </summary>
    /// <typeparam name="T">协议类型</typeparam>
    public interface ITcpClient<T> : IClient<T>, IDisposable where T : PacketBase
    {
        /// <summary>
        /// 连接到远程终端       
        /// </summary>
        /// <param name="ip">远程ip</param>
        /// <param name="port">远程端口</param>
        /// <returns></returns>
        Task<bool> Connect(IPAddress ip, int port);

        /// <summary>
        /// 连接到远程终端 
        /// </summary>
        /// <param name="remoteEndPoint">远程ip和端口</param> 
        /// <returns></returns>
        Task<bool> Connect(IPEndPoint remoteEndPoint);
    }
}
