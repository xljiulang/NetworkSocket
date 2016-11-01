using NetworkSocket.Http;
using System;
using System.Collections.Generic;
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
        /// 换行
        /// </summary>
        private static readonly string CRLF = "\r\n";

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
            var builder = new StringBuilder();
            builder.Append("HTTP/1.1 101 Switching Protocols").Append(CRLF);
            builder.Append("Upgrade: websocket").Append(CRLF);
            builder.Append("Connection: Upgrade").Append(CRLF);
            builder.Append("Sec-WebSocket-Accept: " + this.CreateResponseKey()).Append(CRLF);
            builder.Append("Server: NetworkSocket").Append(CRLF).Append(CRLF);

            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(builder.ToString()));
        }
    }
}
