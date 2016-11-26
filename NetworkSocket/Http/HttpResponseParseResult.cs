using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http回复解析结果
    /// </summary>
    public class HttpResponseParseResult
    {
        /// <summary>
        /// 获取是否为http回复
        /// </summary>
        public bool IsHttp { get; internal set; }

        /// <summary>
        /// 获取回复的http头字节组长度
        /// </summary>
        public int HeaderLength { get; internal set; }

        /// <summary>
        /// 获取http回复的状态码
        /// </summary>
        public int Status { get; internal set; }

        /// <summary>
        /// 获取http回复的状态描述
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// 获取回复头信息
        /// </summary>
        public NameValueCollection Header { get; set; }
    }
}
