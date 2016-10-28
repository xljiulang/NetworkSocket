using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 定义jsonWebsocket协议的Api服务
    /// </summary>
    public interface IJsonWebSocketApiService : IDisposable
    {
        /// <summary>
        /// 异步执行Api行为
        /// </summary>              
        /// <param name="actionContext">Api行为上下文</param>   
        /// <returns></returns>
        Task ExecuteAsync(ActionContext actionContext);
    }
}
