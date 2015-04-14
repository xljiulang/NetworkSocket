using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp客户端接口
    /// </summary>
    public interface IFastTcpClient
    {
        /// <summary>
        /// 获取或设置序列化工具      
        /// </summary>
        ISerializer Serializer { get; set; }

        /// <summary>
        /// 调用服务端实现的服务方法        
        /// </summary>       
        /// <param name="command">服务行为的command值</param>
        /// <param name="parameters">参数列表</param>  
        Task InvokeRemote(int command, params object[] parameters);

        /// <summary>
        /// 调用服务端实现的服务方法    
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="command">服务行为的command值</param>
        /// <param name="parameters">参数</param>   
        /// <returns>远程数据任务</returns>    
        Task<T> InvokeRemote<T>(int command, params object[] parameters);
    }
}
