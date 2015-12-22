using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义监听者的行为
    /// </summary>
    public interface IListener : IDisposable
    {
        /// <summary>
        /// 开始启动监听       
        /// </summary>
        /// <param name="server">服务</param>
        /// <param name="port">端口</param>
        void Start(IServer server ,int port);
    }
}
