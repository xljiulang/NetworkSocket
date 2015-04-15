using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示回复对象抽象类
    /// </summary>
    public abstract class Response : PacketBase
    {
        /// <summary>
        /// 获取原始内容
        /// </summary>
        protected byte[] Content { get; set; }

        /// <summary>
        /// 回复对象
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        /// 回复对象
        /// </summary>       
        /// <param name="content">内容</param>       
        public Response(byte[] content)
        {
            this.Content = content;
        }
    }
}
