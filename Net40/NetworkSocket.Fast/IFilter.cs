using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 滤过器接口
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// 执行顺序
        /// 越小最优先
        /// </summary>
        int Order { get; set; }

        /// <summary>
        /// 在执行服务方法前触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        void OnExecuting(SocketAsync<FastPacket> client, FastPacket packet);

        /// <summary>
        /// 在执行服务方法后触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        void OnExecuted(SocketAsync<FastPacket> client, FastPacket packet);
    }
}
