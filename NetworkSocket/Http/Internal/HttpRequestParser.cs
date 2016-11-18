using NetworkSocket.Exceptions;
using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        private static readonly byte[] DoubleCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");

        /// <summary>
        /// 请求头键值分隔
        /// </summary>
        private static readonly byte[] KvSpliter = Encoding.ASCII.GetBytes(": ");


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
        /// <exception cref="HttpException"></exception>
        /// <returns></returns>
        public static HttpRequestParseResult Parse(IContenxt context)
        {
            var headerLength = 0;
            var result = new HttpRequestParseResult();
            context.StreamReader.Position = 0;

            result.IsHttp = HttpRequestParser.IsHttpRequest(context.StreamReader, out headerLength);
            if (result.IsHttp == false || headerLength <= 0)
            {
                return result;
            }

            var contentLength = 0; 
            var request = HttpRequestParser.GetRequest(context, headerLength, out contentLength);
            if (request == null)
            {
                return result;// 数据未完整                 
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
        /// 是否为http请求协议
        /// </summary>
        /// <param name="streamReader">数据读取器</param>
        /// <param name="headerLength">头数据长度，包括双换行</param>
        /// <returns></returns>
        private static bool IsHttpRequest(ISessionStreamReader streamReader, out int headerLength)
        {
            streamReader.Position = 0;
            var methodLength = HttpRequestParser.GetMethodLength(streamReader);
            var methodName = streamReader.ReadString(Encoding.ASCII, methodLength);

            if (HttpRequestParser.MethodNames.Contains(methodName) == false)
            {
                headerLength = 0;
                return false;
            }

            streamReader.Position = 0;
            var headerIndex = streamReader.IndexOf(HttpRequestParser.DoubleCrlf);
            if (headerIndex < 0)
            {
                headerLength = 0;
                return true;
            }

            headerLength = headerIndex + HttpRequestParser.DoubleCrlf.Length;
            return true;
        }

        /// <summary>
        /// 获取当前的http方法长度
        /// </summary>
        /// <param name="streamReader">数据读取器</param>
        /// <returns></returns>
        private static int GetMethodLength(ISessionStreamReader streamReader)
        {
            var maxLength = Math.Min(streamReader.Length, HttpRequestParser.MedthodMaxLength + 1);
            for (var i = 0; i < maxLength; i++)
            {
                if (streamReader[i] == HttpRequestParser.Space)
                {
                    return i;
                }
            }
            return maxLength;
        }

        /// <summary>
        /// 解析http头
        /// 生成请求对象
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="headerLength">请求头长度</param>
        /// <param name="contentLength">内容长度</param>
        public static HttpRequest GetRequest(IContenxt context, int headerLength, out int contentLength)
        {
            var reader = context.StreamReader;
            reader.Position = 0;

            var spaceIndex = reader.IndexOf(Space);
            var httpMethod = HttpRequestParser.CastHttpMethod(reader.ReadString(Encoding.ASCII, spaceIndex));
            reader.Position += 1;

            spaceIndex = reader.IndexOf(Space);
            var path = reader.ReadString(Encoding.ASCII, spaceIndex);
            reader.Position += reader.IndexOf(CRLF) + CRLF.Length;

            var httpHeader = new HttpHeader();
            while (reader.Position < headerLength)
            {
                var keyLength = reader.IndexOf(KvSpliter);
                if (keyLength <= 0)
                {
                    break;
                }

                var lineLength = reader.IndexOf(CRLF) + CRLF.Length;
                if (lineLength < CRLF.Length)
                {
                    break;
                }

                var key = reader.ReadString(Encoding.ASCII, keyLength);
                reader.Position += KvSpliter.Length;
                var value = reader.ReadString(Encoding.ASCII, lineLength - keyLength - KvSpliter.Length - CRLF.Length);
                reader.Position += CRLF.Length;
                httpHeader.Add(key, value);
            }

            contentLength = 0;
            if (httpMethod != HttpMethod.GET)
            {
                contentLength = httpHeader.TryGet<int>("Content-Length");
                if (reader.Length - headerLength < contentLength)
                {
                    return null;// 数据未完整  
                }
            }

            var request = new HttpRequest
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
            return request;
        }

        /// <summary>
        /// 转换http方法
        /// </summary>
        /// <param name="method">方法字符串</param>
        /// <exception cref="HttpException"></exception>
        /// <returns></returns>
        private static HttpMethod CastHttpMethod(string method)
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
        /// <param name="request"></param>
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
                var headLength = streamReader.IndexOf(HttpRequestParser.DoubleCrlf) + HttpRequestParser.DoubleCrlf.Length;
                if (headLength < HttpRequestParser.DoubleCrlf.Length)
                {
                    break;
                }

                var head = streamReader.ReadString(Encoding.UTF8, headLength);
                var bodyLength = streamReader.IndexOf(boundaryBytes);
                if (bodyLength < 0)
                {
                    break;
                }

                var mHead = new MultipartHead(head);
                if (mHead.IsFile == true)
                {
                    var bytes = streamReader.ReadArray(bodyLength);
                    var file = new HttpFile(mHead, bytes);
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
