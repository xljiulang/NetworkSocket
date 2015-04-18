using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 定义基于Json文本协议通讯的WebSocket服务
    /// </summary>
    public interface IFastWebSocketServer
    {
        /// <summary>
        /// 获取或设置Json序列化工具       
        /// </summary>
        IJsonSerializer JsonSerializer { get; set; }

        /// <summary>
        /// 获取或设置Api行为特性过滤器提供者
        /// </summary>
        IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 调用客户端实现的Api      
        /// </summary>
        /// <param name="client">客户端</param>       
        /// <param name="api">api名</param>
        /// <param name="parameters">参数列表</param>  
        Task InvokeApi(IClient<Response> client, string api, params object[] parameters);

        /// <summary>
        /// 调用客户端实现的Api 
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="api">api名</param>
        /// <param name="parameters">参数</param> 
        /// <returns>远程数据任务</returns>  
        Task<T> InvokeApi<T>(IClient<Response> client, string api, params object[] parameters);
    }
}
