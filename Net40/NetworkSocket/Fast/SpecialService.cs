using NetworkSocket.Fast.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 系统级特殊的服务行为
    /// </summary>
    [SpecialService]
    internal class SpecialService : FastServiceBase
    {
        /// <summary>
        /// 获取服务组件版本号
        /// </summary>      
        /// <returns></returns>
        [Service(Implements.Self, (int)SpecialCommands.Version)]
        public string GetVersion()
        {
            return this.GetType().Assembly.GetName().Version.ToString();
        }
    }
}
