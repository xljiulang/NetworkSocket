using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Filters
{
    /// <summary>
    /// Action过滤器
    /// </summary>
    public interface IActionFilter : IFilter
    {
        /// <summary>
        /// 在执行服务方法前触发       
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        /// <returns></returns>
        void OnExecuting(SocketAsync<FastPacket> client, FastPacket packet);

        /// <summary>
        /// 在执行服务方法后触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        void OnExecuted(SocketAsync<FastPacket> client, FastPacket packet);
    }
}
