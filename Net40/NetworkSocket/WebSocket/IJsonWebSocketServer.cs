using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 定义基于Json文本协议通讯的WebSocket服务
    /// </summary>
    public interface IJsonWebSocketServer : IDependencyResolverSupportable, IFilterSupportable
    {
        /// <summary>
        /// 获取或设置Json序列化工具       
        /// </summary>
        IJsonSerializer JsonSerializer { get; set; }
    }
}
