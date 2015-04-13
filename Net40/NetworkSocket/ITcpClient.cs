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
    public interface ITcpClient<T> : IDisposable, ISocketAsync<T> where T : PacketBase
    {
        /// <summary>      
        /// 断开和远程终端的连接
        /// </summary>
        void Close();

        /// <summary>
        /// 连接到远程终端 
        /// </summary>
        /// <param name="remoteEndPoint">远程ip和端口</param> 
        /// <returns></returns>
        Task<bool> Connect(IPEndPoint remoteEndPoint);
    }
}
