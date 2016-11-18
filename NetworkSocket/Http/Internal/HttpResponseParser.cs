using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示回复解析器
    /// </summary>
    internal static class HttpResponseParser
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
        /// http11
        /// </summary>
        private static readonly byte[] Http11 = Encoding.ASCII.GetBytes("HTTP/1.1");

        /// <summary>
        /// 解析回复头信息        
        /// </summary>
        /// <param name="streamReader">数据读取器</param>   
        /// <returns></returns>
        public static HttpResponseParseResult Parse(ISessionStreamReader streamReader)
        {
            var result = new HttpResponseParseResult();
            streamReader.Position = 0;

            if (streamReader.IndexOf(Http11) != 0)
            {
                return result;
            }

            var index = streamReader.IndexOf(DoubleCrlf);
            if (index < 0)
            {
                return result;
            }

            streamReader.Position += streamReader.IndexOf(Space) + 1;
            var statusLength = streamReader.IndexOf(Space);
            var status = streamReader.ReadString(Encoding.ASCII, statusLength);

            streamReader.Position += 1;
            var descriptionLenth = streamReader.IndexOf(CRLF);
            var description = streamReader.ReadString(Encoding.ASCII, descriptionLenth);

            streamReader.Position += CRLF.Length;
            var httpHeader = new HttpHeader();
            var headerLength = index + DoubleCrlf.Length;
            while (streamReader.Position < headerLength)
            {
                var keyLength = streamReader.IndexOf(KvSpliter);
                if (keyLength <= 0)
                {
                    break;
                }

                var lineLength = streamReader.IndexOf(CRLF) + CRLF.Length;
                if (lineLength < CRLF.Length)
                {
                    break;
                }

                var key = streamReader.ReadString(Encoding.ASCII, keyLength);
                streamReader.Position += KvSpliter.Length;
                var value = streamReader.ReadString(Encoding.ASCII, lineLength - keyLength - KvSpliter.Length - CRLF.Length);
                streamReader.Position += CRLF.Length;
                httpHeader.Add(key, value);
            }

            result.Description = description;
            result.Status = int.Parse(status);
            result.IsHttp = true;
            result.Header = httpHeader;
            return result;
        }
    }
}
