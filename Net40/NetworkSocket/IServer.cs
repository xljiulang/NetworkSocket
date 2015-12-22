using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义服务行为的接口
    /// </summary>
    public interface IServer : IDisposable
    {
        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <param name="middleware">中间件</param>
        void Use(IMiddleware middleware);

        /// <summary>       
        /// 当IListener接受一个新的Socket连接后调用此方法       
        /// 要求服务器开始生成会话并管理这些会话
        /// 当某个会话收到请求后
        /// 生成IContenxt并执行中间件
        /// </summary>
        /// <param name="socket">socket</param>
        /// <param name="socketError">状态</param>    
        /// <returns></returns>
        void OnAccept(Socket socket, SocketError socketError);
    }
}
