using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http请求信息
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// 获取请求的头信息
        /// </summary>
        public HttpHeader Headers { get; private set; }

        /// <summary>
        /// 获取Query
        /// </summary>
        public HttpNameValueCollection Query { get; private set; }

        /// <summary>
        /// 获取Form 
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
        /// 如果数据未完整则返回null
        /// </summary>
        /// <param name="buffer">接收到的原始数量</param>
        /// <param name="localEndpoint">服务器的本地终结点</param>
        /// <param name="remoteEndpoint">远程端的IP和端口</param>
        /// <exception cref="HttpException"></exception>
        /// <returns></returns>
        public static HttpRequest Parse(ReceiveBuffer buffer, IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
        {
            var doubleCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");
            var headerIndex = buffer.IndexOf(doubleCrlf);
            if (headerIndex < 0)
            {
                return null; // 数据未完整
            }

            var headerLength = headerIndex + doubleCrlf.Length;
            var headerString = buffer.GetString(0, headerLength, Encoding.ASCII);
            const string pattern = @"^(?<method>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\r\n" +
                @"((?<field_name>[^:\r\n]+):\s(?<field_value>[^\r\n]*)\r\n)+" +
                @"\r\n";

            var match = Regex.Match(headerString, pattern, RegexOptions.IgnoreCase);
            if (match.Success == false)
            {
                throw new HttpException(400, "请求中有语法问题，或不能满足请求");
            }

            var httpMethod = GetHttpMethod(match.Groups["method"].Value);
            var httpHeader = new HttpHeader(match.Groups["field_name"].Captures, match.Groups["field_value"].Captures);
            var contentLength = httpHeader.TryGet<int>("Content-Length"); ;

            if (httpMethod == HttpMethod.POST && buffer.Length - headerLength < contentLength)
            {
                return null; // 数据未完整
            }

            var request = new HttpRequest
            {
                LocalEndPoint = localEndpoint,
                RemoteEndPoint = remoteEndpoint,
                HttpMethod = httpMethod,
                Headers = httpHeader
            };

            request.Url = new Uri("http://localhost:" + localEndpoint.Port + match.Groups["path"].Value);
            request.Path = request.Url.AbsolutePath;
            request.Query = new HttpNameValueCollection(request.Url.Query.TrimStart('?'));

            var formString = string.Empty;
            if (httpMethod == HttpMethod.POST)
            {
                buffer.Position = headerLength;
                request.InputStrem = buffer.ReadArray(contentLength);
                if (StringEquals(request.Headers["Content-Type"], "application/x-www-form-urlencoded") == true)
                {
                    formString = HttpUtility.UrlDecode(Encoding.UTF8.GetString(request.InputStrem));
                }
            }

            request.Form = new HttpNameValueCollection(formString);
            buffer.Clear(headerLength + contentLength);
            return request;
        }

        /// <summary>
        /// 获取http方法
        /// </summary>
        /// <param name="method">方法字符串</param>
        /// <exception cref="HttpException"></exception>
        /// <returns></returns>
        private static HttpMethod GetHttpMethod(string method)
        {
            var httpMethod = HttpMethod.GET;
            if (Enum.TryParse<HttpMethod>(method, true, out httpMethod))
            {
                return httpMethod;
            }
            throw new HttpException(501, "不支持的http方法：" + method);
        }
    }
}

