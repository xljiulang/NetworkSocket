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
        /// 获取双换行
        /// </summary>
        private static readonly byte[] DoubleCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");

        /// <summary>
        /// 解析回复头信息        
        /// </summary>
        /// <param name="streamReader">数据读取器</param>   
        /// <returns></returns>
        public static HttpResponseParseResult Parse(ISessionStreamReader streamReader)
        {
            var result = new HttpResponseParseResult();
            streamReader.Position = 0;
            var index = streamReader.IndexOf(DoubleCrlf);
            if (index < 0)
            {
                return result;
            }

            var length = index + DoubleCrlf.Length;
            var header = streamReader.ReadString(Encoding.ASCII, length);
            const string pattern = @"^HTTP\/1\.1\s(?<status>[^\s]+)\s(?<description>.*)\r\n" +
                @"((?<field_name>[^:\r\n]+):\s(?<field_value>[^\r\n]*)\r\n)+" +
                @"\r\n";

            var match = Regex.Match(header, pattern, RegexOptions.IgnoreCase);
            if (match.Success == true)
            {
                result.IsHttp = true;
                result.HeaderLength = length;
                result.Description = match.Groups["description"].Value;
                result.Header = HttpHeader.Parse(match.Groups["field_name"].Captures, match.Groups["field_value"].Captures);
            }

            int status = 0;
            int.TryParse(match.Groups["status"].Value, out status);
            result.Status = status;
            return result;
        }
    }
}
