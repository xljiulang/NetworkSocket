using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// FastWebSocket的会话接口
    /// </summary>
    public interface IFastWebSocketSession
    {
        /// <summary>
        /// 调用远程端实现的服务方法        
        /// </summary>       
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param> 
        void InvokeApi(string api, params object[] parameters);

        /// <summary>
        /// 调用客户端实现的服务方法     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>        
        /// <param name="api">api</param>
        /// <param name="parameters">参数</param>
        /// <returns>远程数据任务</returns>  
        Task<T> InvokeApi<T>(string api, params object[] parameters);
    }
}
