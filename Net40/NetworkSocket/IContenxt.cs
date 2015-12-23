using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义Socket的上下文
    /// </summary>
    public interface IContenxt
    {
        /// <summary>
        /// 获取当前会话对象
        /// </summary>
        ISession Session { get; }

        /// <summary>
        /// 获取当前会话收到的历史数据
        /// </summary>
        IReceiveBuffer Buffer { get; }

        /// <summary>
        /// 获取所有会话对象
        /// </summary>
        ISessionProvider AllSessions { get; }
    }
}
