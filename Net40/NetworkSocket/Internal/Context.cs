using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示上下文对象
    /// </summary>
    internal class Context : IContenxt
    {
        /// <summary>
        /// 历史数据对象上下文名称
        /// </summary>
        private static readonly string contextName = "NsStreamContext";

        /// <summary>
        /// 获取或设置当前会话收到的历史数据对象
        /// </summary>
        public INsStream Stream
        {
            get
            {
                return CallContext.GetData(contextName) as INsStream;
            }
            set
            {
                CallContext.SetData(contextName, value);
            }
        }

        /// <summary>
        /// 获取或设置当前会话对象
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// 获取或设置所有会话对象
        /// </summary>
        public ISessionProvider AllSessions { get; set; }
    }
}
