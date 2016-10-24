using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// http请求解析结果
    /// </summary>
    internal class HttpParseResult
    {
        /// <summary>
        /// 是否为http请求
        /// </summary>
        public bool IsHttp { get; set; }

        /// <summary>
        /// 请求的包数据长度
        /// </summary>
        public int PackageLength { get; set; }

        /// <summary>
        /// 请求对象，如果数据未完成，则为null
        /// </summary>
        public HttpRequest Request { get; set; }
    }
}
