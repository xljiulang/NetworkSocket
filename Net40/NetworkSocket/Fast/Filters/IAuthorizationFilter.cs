using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Filters
{
    /// <summary>
    /// 权限过虑器
    /// </summary>
    public interface IAuthorizationFilter : IFilter
    {
        /// <summary>
        /// 授权触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        void OnAuthorization(SocketAsync<FastPacket> client, FastPacket packet);
    }
}
