using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http请求信息
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// 获取Form
        /// </summary>
        public HttpHeader Headers { get; private set; }

        /// <summary>
        /// 获取Query
        /// </summary>
        public HttpNameValueCollection Query { get; private set; }

        /// <summary>
        /// 获取请求的头信息
        /// </summary>
        public HttpNameValueCollection Form { get; private set; }

        /// <summary>
        /// 获取请求的流
        /// </summary>
        public byte[] InputStrem { get; private set; }

        /// <summary>
        /// 获取请求方法
        /// </summary>
        public HttpMethod HttpMethod { get; private set; }

        /// <summary>
        /// 获取请求路径
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 获取请求的Uri
        /// </summary>
        public Uri Url { get; private set; }

        /// <summary>
        /// 获取监听的本地IP和端口
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// 获取远程端的IP和端口
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// Http请求信息
        /// </summary>
        private HttpRequest()
        {
        }

        /// <summary>
        /// 从Query和Form获取请求参数的值
        /// 多个值会以逗号分隔
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                var value = this.Query[key];
                if (string.IsNullOrEmpty(value) == true)
                {
                    value = this.Form[key];
                }
                return value;
            }
        }

        /// <summary>
        /// 从Query和Form获取请求参数的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public IList<string> GetValues(string key)
        {
            var queryValues = this.Query.GetValues(key);
            var formValues = this.Form.GetValues(key);

            var list = new List<string>();
            if (queryValues != null)
            {
                list.AddRange(queryValues);
            }
            if (formValues != null)
            {
                list.AddRange(formValues);
            }
            return list;
        }

        /// <summary>
        /// 获取是否为Websocket请求
        /// </summary>
        /// <returns></returns>
        public bool IsWebsocketRequest()
        {
            if (this.HttpMethod != Http.HttpMethod.GET)
            {
                return false;
            }
            if (StringEquals(this.Headers.TryGet<string>("Connection"), "Upgrade") == false)
            {
                return false;
            }
            if (this.Headers.TryGet<string>("Upgrade") == null)
            {
                return false;
            }
            if (StringEquals(this.Headers.TryGet<string>("Sec-WebSocket-Version"), "13") == false)
            {
                return false;
            }
            if (this.Headers.TryGet<string>("Sec-WebSocket-Key") == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 是否为ajax请求
        /// </summary>
        /// <returns></returns>
        public bool IsAjaxRequest()
        {
            return this["X-Requested-With"] == "XMLHttpRequest" || this.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// 获取是否相等
        /// 不区分大小写
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        private static bool StringEquals(string value1, string value2)
        {
            return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 解析连接请求信息
        /// </summary>
        /// <param name="buffer">接收到的原始数量</param>
        /// <param name="localEndpoint">服务器的本地终结点</param>
        /// <param name="remoteEndpoint">远程端的IP和端口</param>
        /// <returns></returns>
        public static HttpRequest From(ReceiveBuffer buffer, IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
        {
            buffer.Position = 0;
            var bytes = buffer.ReadArray();

            var request = HttpRequest.From(bytes, localEndpoint, remoteEndpoint);
            if (request != null)
            {
                buffer.Clear();
            }
            return request;
        }

        /// <summary>
        /// 解析连接请求信息
        /// </summary>
        /// <param name="bytes">原始数量</param>
        /// <param name="localEndpoint">服务器终结点</param>
        /// <param name="remoteEndpoint">远程端的IP和端口</param>
        /// <returns></returns>
        private static HttpRequest From(byte[] bytes, IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
        {
            const string pattern = @"^(?<method>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\r\n" +
                @"((?<field_name>[^:\r\n]+):\s(?<field_value>[^\r\n]*)\r\n)+" +
                @"\r\n";

            var match = Regex.Match(Encoding.ASCII.GetString(bytes), pattern, RegexOptions.IgnoreCase);
            if (match.Success == false)
            {
                return null;
            }

            var httpMethod = HttpRequest.GetHttpMethod(match.Groups["method"].Value);
            var httpHeader = new HttpHeader(match.Groups["field_name"].Captures, match.Groups["field_value"].Captures);
            if (httpMethod != HttpMethod.GET && bytes.Length - match.Length < httpHeader.ContentLength)
            {
                return null;
            }

            var request = new HttpRequest
            {
                InputStrem = bytes,
                LocalEndPoint = localEndpoint,
                RemoteEndPoint = remoteEndpoint,
                HttpMethod = httpMethod,
                Headers = httpHeader
            };

            request.Url = new Uri("http://localhost:" + localEndpoint.Port + match.Groups["path"].Value);
            request.Path = request.Url.AbsolutePath;
            request.Query = new HttpNameValueCollection(request.Url.Query.TrimStart('?'));

            var form = string.Empty;
            if (httpMethod != HttpMethod.GET)
            {
                var contentType = request.Headers.TryGet<string>("Content-Type");
                if (StringEquals(contentType, "application/x-www-form-urlencoded") == true)
                {
                    form = Encoding.UTF8.GetString(bytes, match.Length, bytes.Length - match.Length);
                }
            }
            request.Form = new HttpNameValueCollection(form);
            return request;
        }

        /// <summary>
        /// 获取请求方式
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static HttpMethod GetHttpMethod(string method)
        {
            var httpMethod = HttpMethod.GET;
            Enum.TryParse<HttpMethod>(method, true, out httpMethod);
            return httpMethod;
        }
    }
}

