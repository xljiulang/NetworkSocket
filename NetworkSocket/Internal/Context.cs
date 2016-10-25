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
        /// 获取或设置当前会话对象
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// 获取当前会话收到的数据
        /// </summary>
        public IStreamReader InputStream { get; set; }      

        /// <summary>
        /// 获取或设置所有会话对象
        /// </summary>
        public ISessionManager AllSessions { get; set; }
    }
}
