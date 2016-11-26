using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示Websocket的握手回复
    /// </summary>
    public class HandshakeResponse : WebsocketResponse
    {
        /// <summary>
        /// Sec-WebSocket-Key
        /// </summary>
        private string secValue;

        /// <summary>
        /// 表示握手回复
        /// </summary>
        /// <param name="secValue">Sec-WebSocket-Key</param>
        public HandshakeResponse(string secValue)
        {
            this.secValue = secValue;
        }

        /// <summary>
        /// 生成回复的key
        /// </summary>      
        /// <returns></returns>
        private string CreateResponseKey()
        {
            const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(this.secValue + guid));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public override ArraySegment<byte> ToArraySegment(bool mask)
        {
            var builder = HttpHeaderBuilder.Resonse(101, "Switching Protocols");
            builder.Add("Upgrade", "websocket");
            builder.Add("Connection", "Upgrade");
            builder.Add("Sec-WebSocket-Accept", this.CreateResponseKey());
            builder.Add("Server", "NetworkSocket");
            var bytes = builder.ToByteArray();
            return new ArraySegment<byte>(bytes);
        }
    }
}
