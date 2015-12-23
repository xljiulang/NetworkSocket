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
        /// 使用中间件
        /// </summary>
        /// <param name="middleware">中间件</param>
        void Use(IMiddleware middleware);

        /// <summary>
        /// 开始启动监听       
        /// </summary>
        /// <param name="port">端口</param>
        void Start(int port);
    }
}
