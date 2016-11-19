using NetworkSocket.Exceptions;
using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 请求解析器
    /// </summary>
    internal static class HttpRequestParser
    {
        /// <summary>
        /// 空格
        /// </summary>
        private static readonly byte Space = 32;

        /// <summary>
        /// 换行
        /// </summary>
        private static readonly byte[] CRLF = Encoding.ASCII.GetBytes("\r\n");

        /// <summary>
        /// 获取双换行
        /// </summary>
        private static readonly byte[] DoubleCRLF = Encoding.ASCII.GetBytes("\r\n\r\n");

        /// <summary>
        /// 请求头键值分隔
        /// </summary>
        private static readonly byte[] KvSpliter = Encoding.ASCII.GetBytes(": ");

        /// <summary>
        /// http1.1
        /// </summary>
        private static readonly byte[] HttpVersion11 = Encoding.ASCII.GetBytes("HTTP/1.1");

        /// <summary>
        /// 支持的http方法
        /// </summary>
        private static readonly HashSet<string> MethodNames = new HashSet<string>(Enum.GetNames(typeof(HttpMethod)), StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 支持的http方法最大长度
        /// </summary>
        private static readonly int MedthodMaxLength = MethodNames.Max(m => m.Length);



        /// <summary>
        /// 解析连接请求信息        
        /// </summary>
        /// <param name="context">上下文</param>   
        /// <returns></returns>
        public static HttpRequestParseResult Parse(IContenxt context)
        {
            var result = HttpRequestParser.ParseInternal(context);
            context.StreamReader.Position = 0;
            return result;
        }

        /// <summary>
        /// 解析连接请求信息        
        /// </summary>
        /// <param name="context">上下文</param>   
        /// <returns></returns>
        private static HttpRequestParseResult ParseInternal(IContenxt context)
        {
            var headerLength = 0;
            var contentLength = 0;
            var request = default(HttpRequest);
            var result = new HttpRequestParseResult();

            result.IsHttp = HttpRequestParser.GetRequest(context, out request, out headerLength, out contentLength);
            if (result.IsHttp == false)
            {
                return result;
            }

            if (request == null) // 数据未完整     
            {
                return result;
            }

            switch (request.HttpMethod)
            {
                case HttpMethod.GET:
                    request.Body = new byte[0];
                    request.Form = new HttpNameValueCollection();
                    request.Files = new HttpFile[0];
                    break;

                default:
                    context.StreamReader.Position = headerLength;
                    request.Body = context.StreamReader.ReadArray(contentLength);
                    context.StreamReader.Position = headerLength;
                    HttpRequestParser.GeneratePostFormAndFiles(request, context.StreamReader);
                    break;
            }

            result.Request = request;
            result.PackageLength = headerLength + contentLength;
            return result;
        }


        /// <summary>
        /// 解析http头
        /// 生成请求对象
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="request">请求对象</param>
        /// <param name="headerLength">请求头长度</param>
        /// <param name="contentLength">请求内容长度</param>       
        /// <returns></returns>
        public static bool GetRequest(IContenxt context, out HttpRequest request, out int headerLength, out int contentLength)
        {
            request = null;
            headerLength = 0;
            contentLength = 0;
            var reader = context.StreamReader;

            // HTTP Method
            reader.Position = 0;
            var methodLength = reader.IndexOf(Space);
            if (methodLength <= 0 || methodLength > MedthodMaxLength)
            {
                return false;
            }
            var methodName = reader.ReadString(Encoding.ASCII, methodLength);
            if (MethodNames.Contains(methodName) == false)
            {
                return false;
            }
            var httpMethod = (HttpMethod)Enum.Parse(typeof(HttpMethod), methodName, true);

            // HTTP Path
            reader.Position += 1;
            var pathLength = reader.IndexOf(Space);
            if (pathLength < 0)
            {
                return false;
            }
            var path = reader.ReadString(Encoding.ASCII, pathLength);


            // HTTP Version
            reader.Position += 1;
            if (reader.StartWith(HttpVersion11) == false)
            {
                return false;
            }
            reader.Position += HttpVersion11.Length;
            if (reader.StartWith(CRLF) == false)
            {
                return false;
            }

            // HTTP Second line
            reader.Position += CRLF.Length;
            var endIndex = reader.IndexOf(DoubleCRLF);
            if (endIndex < 0)
            {
                return false;
            }

            var httpHeader = new HttpHeader();
            headerLength = reader.Position + endIndex + DoubleCRLF.Length;


            while (reader.Position < headerLength)
            {
                var keyLength = reader.IndexOf(KvSpliter);
                if (keyLength <= 0)
                {
                    break;
                }
                var key = reader.ReadString(Encoding.ASCII, keyLength);

                reader.Position += KvSpliter.Length;
                var valueLength = reader.IndexOf(CRLF);
                if (valueLength < 0)
                {
                    break;
                }
                var value = reader.ReadString(Encoding.ASCII, valueLength);

                if (reader.StartWith(CRLF) == false)
                {
                    break;
                }
                reader.Position += CRLF.Length;
                httpHeader.Add(key, value);
            }

            if (httpMethod != HttpMethod.GET)
            {
                contentLength = httpHeader.TryGet<int>("Content-Length");
                if (reader.Length - headerLength < contentLength)
                {
                    return true;// 数据未完整  
                }
            }

            request = new HttpRequest
            {
                LocalEndPoint = context.Session.LocalEndPoint,
                RemoteEndPoint = context.Session.RemoteEndPoint,
                HttpMethod = httpMethod,
                Headers = httpHeader
            };

            var scheme = context.Session.IsSecurity ? "https" : "http";
            var host = httpHeader["Host"];
            if (string.IsNullOrEmpty(host) == true)
            {
                host = context.Session.LocalEndPoint.ToString();
            }
            request.Url = new Uri(string.Format("{0}://{1}{2}", scheme, host, path));
            request.Path = request.Url.AbsolutePath;
            request.Query = HttpNameValueCollection.Parse(request.Url.Query.TrimStart('?'));
            return true;
        }


        /// <summary>
        /// 生成Post得到的表单和文件
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="streamReader">数据读取器</param>      
        private static void GeneratePostFormAndFiles(HttpRequest request, ISessionStreamReader streamReader)
        {
            var boundary = default(string);
            if (request.IsApplicationFormRequest() == true)
            {
                HttpRequestParser.GenerateApplicationForm(request);
            }
            else if (request.IsMultipartFormRequest(out boundary) == true)
            {
                if (request.Body.Length >= boundary.Length)
                {
                    HttpRequestParser.GenerateMultipartFormAndFiles(request, streamReader, boundary);
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
        /// <param name="request">请求对象</param>
        private static void GenerateApplicationForm(HttpRequest request)
        {
            var body = Encoding.UTF8.GetString(request.Body);
            request.Form = HttpNameValueCollection.Parse(body);
            request.Files = new HttpFile[0];
        }

        /// <summary>
        /// 生成表单和文件
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="streamReader">数据读取器</param>   
        /// <param name="boundary">边界</param>
        private static void GenerateMultipartFormAndFiles(HttpRequest request, ISessionStreamReader streamReader, string boundary)
        {
            var boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary);
            var maxPosition = streamReader.Length - Encoding.ASCII.GetBytes("--\r\n").Length;

            var files = new List<HttpFile>();
            var form = new HttpNameValueCollection();

            streamReader.Position = streamReader.Position + boundaryBytes.Length;
            while (streamReader.Position < maxPosition)
            {
                var headLength = streamReader.IndexOf(DoubleCRLF) + DoubleCRLF.Length;
                if (headLength < DoubleCRLF.Length)
                {
                    break;
                }

                var head = streamReader.ReadString(Encoding.UTF8, headLength);
                var bodyLength = streamReader.IndexOf(boundaryBytes);
                if (bodyLength < 0)
                {
                    break;
                }

                string fileName = null;
                var mHead = new MultipartHead(head);

                if (mHead.TryGetFileName(out fileName) == true)
                {
                    var bytes = streamReader.ReadArray(bodyLength);
                    var file = new HttpFile(mHead.Name, fileName, bytes);
                    files.Add(file);
                }
                else
                {
                    var byes = streamReader.ReadArray(bodyLength);
                    var value = HttpUtility.UrlDecode(byes, Encoding.UTF8);
                    form.Add(mHead.Name, value);
                }
                streamReader.Position = streamReader.Position + boundaryBytes.Length;
            }

            request.Form = form;
            request.Files = files.ToArray();
        }
    }
}
