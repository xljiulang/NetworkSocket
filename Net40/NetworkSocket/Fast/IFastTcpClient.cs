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
    public interface IFastTcpClient : ITcpClient<FastPacket>
    {
        /// <summary>
        /// 获取或设置序列化工具      
        /// </summary>
        ISerializer Serializer { get; set; }

        /// <summary>
        /// 调用服务端实现的Api       
        /// </summary>       
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param>  
        Task InvokeApi(string api, params object[] parameters);

        /// <summary>
        /// 调用服务端实现的Api    
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="api">api</param>
        /// <param name="parameters">参数</param>   
        /// <returns>远程数据任务</returns>    
        Task<T> InvokeApi<T>(string api, params object[] parameters);
    }
}
