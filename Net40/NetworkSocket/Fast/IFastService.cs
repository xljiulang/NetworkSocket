using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 定义快速服务的执行
    /// </summary>
    public interface IFastService : IDisposable
    {
        /// <summary>
        /// 获取或设置关联的TcpServer
        /// </summary>
        IFastTcpServer FastTcpServer { get; set; }

        /// <summary>
        /// 执行服务行为
        /// </summary>       
        /// <param name="actionContext">上下文</param>      
        void Execute(ActionContext actionContext);
    }
}
