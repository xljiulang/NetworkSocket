using System.Collections.Specialized;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http回复解析结果
    /// </summary>
    class HttpResponseParseResult
    {
        /// <summary>
        /// 是否为http请求
        /// </summary>
        public bool IsHttp { get; set; }

        /// <summary>
        /// 回复头长度
        /// </summary>
        public int HeaderLength { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        public NameValueCollection Header { get; set; }
    }
}
