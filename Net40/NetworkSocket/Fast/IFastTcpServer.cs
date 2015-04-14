using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp服务端接口
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
        /// 调用客户端实现的服务方法        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="command">数据包的command值</param>
        /// <param name="parameters">参数列表</param>  
        void InvokeRemote(IClient<FastPacket> client, int command, params object[] parameters);

        /// <summary>
        /// 调用客户端实现的服务方法      
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="command">数据包的命令值</param>
        /// <param name="parameters">参数</param> 
        /// <returns>远程数据任务</returns>  
        Task<T> InvokeRemote<T>(IClient<FastPacket> client, int command, params object[] parameters);
    }
}
