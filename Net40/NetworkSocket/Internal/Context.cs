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
        /// 会话收到的数据
        /// </summary>
        [ThreadStatic]
        private static IReceiveBuffer buffer;

        /// <summary>
        /// 获取或设置当前会话对象
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// 获取或设置当前会话收到的历史数据对象
        /// </summary>
        public IReceiveBuffer Buffer
        {
            get
            {
                return Context.buffer;
            }
            set
            {
                Context.buffer = value;
            }
        }

        /// <summary>
        /// 获取或设置所有会话对象
        /// </summary>
        public ISessionProvider AllSessions { get; set; }
    }
}
