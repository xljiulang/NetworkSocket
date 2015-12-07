using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 定义jsonWebsocket协议的Api服务
    /// </summary>
    public interface IJsonWebSocketApiService : IDisposable
    {
        /// <summary>
        /// 执行Api行为
        /// </summary>              
        /// <param name="actionContext">Api行为上下文</param>      
        void Execute(ActionContext actionContext);
    }
}
