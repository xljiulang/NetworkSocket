using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 定义快速构建Tcp的服务器
    /// </summary>
    public interface IFastTcpServer : ITcpServer<FastPacket>
    {
        /// <summary>
        /// 获取或设置序列化工具      
        /// </summary>
        ISerializer Serializer { get; set; }

        /// <summary>
        /// 获取或设置服务行为特性过滤器提供者
        /// </summary>
        IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 获取服务实例
        /// 并赋值给服务实例的FastTcpServer属性
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        T GetService<T>() where T : IFastService;
    }
}
