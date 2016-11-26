using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http请求解析结果
    /// </summary>
    public class HttpRequestParseResult
    {
        /// <summary>
        /// 获取是否为http请求
        /// </summary>
        public bool IsHttp { get; internal set; }

        /// <summary>
        /// 获取http请求指定的请求字节组长度
        /// </summary>
        public int RequestLength { get; internal set; }

        /// <summary>
        /// 获取请求对象，如果请求的数据未完整，则值为null
        /// </summary>
        public HttpRequest Request { get; internal set; }
    }
}
