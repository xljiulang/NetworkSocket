using NetworkSocket.Fast.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 特殊的服务方法
    /// </summary>
    [SpecialService]
    internal class SpecialService : FastServiceBase
    {
        /// <summary>
        /// 获取服务组件版本号
        /// </summary>
        /// <param name="client">客户端</param>
        /// <returns></returns>
        [Service(Implements.Self, (int)SpecialCommands.Version)]
        public string GetVersion(SocketAsync<FastPacket> client)
        {
            return this.GetType().Assembly.GetName().Version.ToString();
        }
    }
}
