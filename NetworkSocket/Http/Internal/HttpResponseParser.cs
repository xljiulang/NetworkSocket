using System.Text;

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
        private static readonly byte[] DoubleCRLF = Encoding.ASCII.GetBytes("\r\n\r\n");

        /// <summary>
        /// 请求头键值分隔
        /// </summary>
        private static readonly byte[] KvSpliter = Encoding.ASCII.GetBytes(": ");

        /// <summary>
        /// http11
        /// </summary>
        private static readonly byte[] HttpVersion11 = Encoding.ASCII.GetBytes("HTTP/1.1");

        /// <summary>
        /// 解析回复头信息        
        /// </summary>
        /// <param name="streamReader">数据读取器</param>   
        /// <returns></returns>
        public static HttpResponseParseResult Parse(ISessionStreamReader streamReader)
        {
            var result = new HttpResponseParseResult();
            streamReader.Position = 0;

            if (streamReader.StartWith(HttpVersion11) == false)
            {
                return result;
            }

            var endIndex = streamReader.IndexOf(DoubleCRLF);
            if (endIndex < 0)
            {
                return result;
            }

            streamReader.Position += HttpVersion11.Length + 1;
            var statusLength = streamReader.IndexOf(Space);
            if (statusLength < 0)
            {
                return result;
            }
            var status = streamReader.ReadString(Encoding.ASCII, statusLength);

            streamReader.Position += 1;
            var descriptionLenth = streamReader.IndexOf(CRLF);
            if (descriptionLenth < 0)
            {
                return result;
            }
            var description = streamReader.ReadString(Encoding.ASCII, descriptionLenth);

            streamReader.Position += CRLF.Length;
            var httpHeader = new HttpHeader();
            var headerLength = endIndex + DoubleCRLF.Length;
            while (streamReader.Position < headerLength)
            {
                var keyLength = streamReader.IndexOf(KvSpliter);
                if (keyLength <= 0)
                {
                    break;
                }
                var key = streamReader.ReadString(Encoding.ASCII, keyLength);

                streamReader.Position += KvSpliter.Length;
                var valueLength = streamReader.IndexOf(CRLF);
                if (valueLength < 0)
                {
                    break;
                }
                var value = streamReader.ReadString(Encoding.ASCII, valueLength);

                if (streamReader.StartWith(CRLF) == false)
                {
                    break;
                }
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
