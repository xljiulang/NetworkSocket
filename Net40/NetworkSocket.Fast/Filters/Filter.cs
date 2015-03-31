using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Filters
{
    /// <summary>
    /// 过滤器
    /// </summary>
    public class Filter : IFilter
    {
        /// <summary>
        /// 获取或设置过滤器特性
        /// </summary>
        public FilterAttribute FilterAttribute { get; set; }

        /// <summary>
        /// 获取中设置排序
        /// </summary>
        public int Order
        {
            get
            {
                return this.FilterAttribute.Order;
            }
            set
            {
                this.FilterAttribute.Order = value;
            }
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        public void OnExecuting(SocketAsync<FastPacket> client, FastPacket packet)
        {
            this.FilterAttribute.OnExecuting(client, packet);
        }

        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        public void OnExecuted(SocketAsync<FastPacket> client, FastPacket packet)
        {
            this.FilterAttribute.OnExecuted(client, packet);
        }
    }
}
