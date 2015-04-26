using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Fast会话接口
    /// </summary>
    public interface IFastSession
    {
        /// <summary>
        /// 调用远程端实现的Api        
        /// </summary>        
        /// <param name="api">数据包Api名</param>
        /// <param name="parameters">参数列表</param> 
        void InvokeApi(string api, params object[] parameters);

        /// <summary>
        /// 调用远程端实现的Api      
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>        
        /// <param name="api">数据包Api名</param>
        /// <param name="parameters">参数</param>        
        /// <returns>远程数据任务</returns>         
        Task<T> InvokeApi<T>(string api, params object[] parameters);
    }
}
