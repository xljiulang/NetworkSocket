using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Json
{
    /// <summary>
    /// 定义Json服务的执行
    /// </summary>
    public interface IJsonService : IDisposable
    {
        /// <summary>
        /// 执行服务行为
        /// </summary>              
        /// <param name="actionContext">服务行为上下文</param>      
        void Execute(ActionContext actionContext);
    }
}
