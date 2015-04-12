using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示服务行为标记特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// 获取实现服务的目标
        /// </summary>
        public Implements Implement { get; private set; }

        /// <summary>
        /// 获取服务行为对应的数据包命令值
        /// </summary>
        public int Command { get; private set; }

        /// <summary>
        /// 表示服务行为标记特性
        /// </summary>       
        /// <param name="implement">实现服务的目标</param>
        /// <param name="command">服务行为对应的数据包命令值</param>
        public ServiceAttribute(Implements implement, int command)
        {
            this.Implement = implement;
            this.Command = command;
        }
    }
}
