using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Attributes
{
    /// <summary>
    /// 表示服务器服务方法过滤器基础特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class FilterAttribute : Attribute
    {
        /// <summary>
        /// 执行顺序
        /// 越小最优先
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 在执行服务方法前触发
        /// 如果返回false，将不执行服务方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        /// <returns></returns>
        public abstract bool OnExecuting(SocketAsync<FastPacket> client, FastPacket packet);
    }
}
