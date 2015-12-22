using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示上下文对象
    /// </summary>
    internal class Context : IContenxt
    {
        /// <summary>
        /// 获取当前会话对象
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// 获取当前会话收到的历史数据对象
        /// </summary>
        public IReceiveStream Buffer { get; private set; }

        /// <summary>
        /// 获取所有会话对象
        /// </summary>
        public ISessionManager AllSessions { get; private set; }

        /// <summary>
        /// 上下文对象
        /// </summary>
        /// <param name="session">当前会话对象</param>
        /// <param name="buffer">当前会话收到的历史数据对象</param>
        /// <param name="allSessions">所有会话对象</param>       
        public Context(ISession session, IReceiveStream buffer, ISessionManager allSessions)
        {
            this.Session = session;
            this.Buffer = buffer;
            this.AllSessions = allSessions;
        }
    }
}
