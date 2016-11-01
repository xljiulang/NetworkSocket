using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示Websocekt回复对象抽象类
    /// </summary>
    public abstract class WebsocketResponse
    {
        /// <summary>
        /// 转换为ArraySegment
        /// </summary>
        /// <param name="mask">是否打码</param>
        /// <returns></returns>
        public abstract ArraySegment<byte> ToArraySegment(bool mask);
    }
}
