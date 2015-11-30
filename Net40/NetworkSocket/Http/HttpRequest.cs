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
        /// 获取请求的文件
        /// </summary>
        public HttpFile[] Files { get; private set; }

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
        /// Content-Type是否为
        /// application/x-www-form-urlencoded
        /// </summary>
        /// <returns></returns>
        public bool IsApplicationFormRequest()
        {
            var contentType = this.Headers["Content-Type"];
            return StringEquals(contentType, "application/x-www-form-urlencoded");
        }

        /// <summary>
        /// Content-Type是否为
        /// multipart/form-data
        /// </summary>
        /// <returns></returns>
        public bool IsMultipartFormRequest(out string boundary)
        {
            var contentType = this.Headers["Content-Type"];
            var match = Regex.Match(contentType, "(?<=multipart/form-data; boundary=).+");
            boundary = match.Value;
            return match.Success;
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
            buffer.Position = 0;
            var doubleCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");
            var headerIndex = buffer.IndexOf(doubleCrlf);
            if (headerIndex < 0)
            {
                return null; // 数据未完整
            }

            var headerLength = headerIndex + doubleCrlf.Length;
            var headerString = buffer.ReadString(headerLength, Encoding.ASCII);
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
            var contentLength = httpHeader.TryGet<int>("Content-Length");

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

            if (httpMethod == HttpMethod.GET)
            {
                request.InputStrem = new byte[0];
                request.Form = new HttpNameValueCollection();
                request.Files = new HttpFile[0];
            }
            else
            {
                request.InputStrem = buffer.ReadArray(contentLength);
                buffer.Position = headerLength;
                HttpRequest.GeneratePostFormAndFiles(request, buffer);
            }

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

        /// <summary>
        /// 生成Post得到的表单和文件
        /// </summary>
        /// <param name="request"></param>
        /// <param name="buffer"></param>      
        private static void GeneratePostFormAndFiles(HttpRequest request, ReceiveBuffer buffer)
        {
            var boundary = default(string);
            if (request.IsApplicationFormRequest() == true)
            {
                HttpRequest.GenerateApplicationForm(request);
            }
            else if (request.IsMultipartFormRequest(out boundary) == true)
            {
                if (request.InputStrem.Length >= boundary.Length)
                {
                    HttpRequest.GenerateMultipartFormAndFiles(request, buffer, boundary);
                }
            }


            if (request.Form == null)
            {
                request.Form = new HttpNameValueCollection();
            }

            if (request.Files == null)
            {
                request.Files = new HttpFile[0];
            }
        }

        /// <summary>
        /// 生成一般表单的Form
        /// </summary>
        /// <param name="request"></param>
        private static void GenerateApplicationForm(HttpRequest request)
        {
            var formString = HttpUtility.UrlDecode(Encoding.UTF8.GetString(request.InputStrem));
            request.Form = new HttpNameValueCollection(formString);
            request.Files = new HttpFile[0];
        }

        /// <summary>
        /// 生成表单和文件
        /// </summary>
        /// <param name="request"></param>
        /// <param name="buffer"></param>   
        /// <param name="boundary">边界</param>
        private static void GenerateMultipartFormAndFiles(HttpRequest request, ReceiveBuffer buffer, string boundary)
        {
            var doubleCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");
            var boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary);
            var maxPosition = buffer.Length - Encoding.ASCII.GetBytes("--\r\n").Length;

            var files = new List<HttpFile>();
            var form = new HttpNameValueCollection();

            buffer.Position = buffer.Position + boundaryBytes.Length;
            while (buffer.Position < maxPosition)
            {
                var headLength = buffer.IndexOf(doubleCrlf) + doubleCrlf.Length;
                if (headLength < doubleCrlf.Length)
                {
                    break;
                }

                var head = buffer.ReadString(headLength, Encoding.UTF8);
                var bodyLength = buffer.IndexOf(boundaryBytes);
                if (bodyLength < 0)
                {
                    break;
                }

                var mHead = new MultipartHead(head);
                if (mHead.IsFile == true)
                {
                    var stream = buffer.ReadArray(bodyLength);
                    var file = new HttpFile(mHead.Name, mHead.FileName, stream);
                    files.Add(file);
                }
                else
                {
                    var value = buffer.ReadString(bodyLength, Encoding.UTF8);
                    form.Add(mHead.Name, value);
                }
                buffer.Position = buffer.Position + boundaryBytes.Length;
            }

            request.Form = form;
            request.Files = files.ToArray();
        }
    }
}

