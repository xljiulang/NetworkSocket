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
        public IDictionary<string, string> Header { get; private set; }

        /// <summary>
        /// 请求方法
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
        /// 获取头数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                string value = null;
                if (this.Header.TryGetValue(key, out value))
                {
                    return value;
                }
                return null;
            }
        }

        /// <summary>
        /// 握手请求信息
        /// </summary>
        private HandshakeRequest()
        {
            this.Header = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取头数据是否存在
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public bool ExistHeader(string key, string value)
        {
            return string.Equals(this[key], value, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// 解析连接请求信息
        /// </summary>
        /// <param name="buffer">接收到的原始数量</param>
        /// <returns></returns>
        public static HandshakeRequest From(ReceiveBuffer buffer)
        {
            buffer.Position = 0;
            var bytes = buffer.ReadArray();
            buffer.Clear();
            return HandshakeRequest.From(bytes, "ws");
        }

        /// <summary>
        /// 解析连接请求信息
        /// </summary>
        /// <param name="bytes">原始数量</param>
        /// <param name="scheme">scheme</param>
        /// <returns></returns>
        private static HandshakeRequest From(byte[] bytes, string scheme)
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

