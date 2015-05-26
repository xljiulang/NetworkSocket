using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示握手回复
    /// </summary>
    public class HandshakeResponse : Response
    {
        /// <summary>
        /// 握手请求
        /// </summary>
        private HttpRequest request;

        /// <summary>
        /// 表示握手回复
        /// </summary>
        /// <param name="request">握手请求</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HandshakeResponse(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException();
            }
            this.request = request;
        }


        /// <summary>
        /// 生成回复的key
        /// </summary>      
        /// <returns></returns>
        private string CreateResponseKey()
        {
            var secKey = this.request["Sec-WebSocket-Key"];
            var guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(secKey + guid));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public override ByteRange ToByteRange()
        {
            var builder = new StringBuilder();
            builder.AppendLine("HTTP/1.1 101 Switching Protocols");
            builder.AppendLine("Upgrade: websocket");
            builder.AppendLine("Connection: Upgrade");
            builder.AppendLine("Sec-WebSocket-Accept: " + this.CreateResponseKey());
            builder.AppendLine("Server: NetworkSocket.WebSocket");
            builder.AppendLine();
            return new ByteRange(Encoding.UTF8.GetBytes(builder.ToString()));
        }
    }
}
