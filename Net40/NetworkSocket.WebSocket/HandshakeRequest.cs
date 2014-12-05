using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示握手请求信息
    /// </summary>
    public class HandshakeRequest
    {
        /// <summary>
        /// 头信息
        /// </summary>
        public readonly IDictionary<string, string> Header;
        /// <summary>
        /// 请求方法eg.Get
        /// </summary>
        public string Method { get; private set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// 数据体
        /// </summary>
        public string Body { get; private set; }
        /// <summary>
        /// Scheme
        /// </summary>
        public string Scheme { get; private set; }

        /// <summary>
        /// 握手请求信息
        /// </summary>
        private HandshakeRequest()
        {
            this.Header = new Dictionary<string, string>(System.StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 生成握手内容
        /// </summary>
        /// <returns></returns>
        public byte[] ToHandshake()
        {
            var builder = new StringBuilder();
            builder.AppendLine("HTTP/1.1 101 Switching Protocols");
            builder.AppendLine("Upgrade: websocket");
            builder.AppendLine("Connection: Upgrade");
            var responseKey = this.CreateResponseKey(this.Header["Sec-WebSocket-Key"]);
            builder.AppendLine("Sec-WebSocket-Accept: " + responseKey);
            builder.AppendLine();
            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        /// <summary>
        /// 生成回复的key
        /// </summary>
        /// <param name="secKey">安全key</param>
        /// <returns></returns>
        private string CreateResponseKey(string secKey)
        {
            const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(secKey + guid));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 解析连接请求信息
        /// </summary>
        /// <param name="bytes">原始数量</param>
        /// <returns></returns>
        public static HandshakeRequest Parse(byte[] bytes)
        {
            return Parse(bytes, "ws");
        }

        /// <summary>
        /// 解析连接请求信息
        /// </summary>
        /// <param name="bytes">原始数量</param>
        /// <param name="scheme">scheme</param>
        /// <returns></returns>
        public static HandshakeRequest Parse(byte[] bytes, string scheme)
        {
            const string pattern = @"^(?<method>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\r\n" +
                @"((?<field_name>[^:\r\n]+):\s(?<field_value>[^\r\n]*)\r\n)+" +
                @"\r\n" +
                @"(?<body>.+)?";

            var match = Regex.Match(Encoding.UTF8.GetString(bytes), pattern, RegexOptions.IgnoreCase);
            if (match.Success == false)
            {
                return null;
            }

            var request = new HandshakeRequest
            {
                Method = match.Groups["method"].Value,
                Path = match.Groups["path"].Value,
                Body = match.Groups["body"].Value.Trim(),
                Scheme = scheme
            };

            var fields = match.Groups["field_name"].Captures;
            var values = match.Groups["field_value"].Captures;
            for (var i = 0; i < fields.Count; i++)
            {
                var name = fields[i].ToString();
                var value = values[i].ToString();
                request.Header[name] = value;
            }
            return request;
        }
    }
}

