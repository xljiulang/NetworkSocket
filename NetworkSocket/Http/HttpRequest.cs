using NetworkSocket.Exceptions;
using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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
        public HttpHeader Headers { get; internal set; }

        /// <summary>
        /// 获取Query
        /// </summary>
        public HttpNameValueCollection Query { get; internal set; }

        /// <summary>
        /// 获取Form 
        /// </summary>
        public HttpNameValueCollection Form { get;  internal set; }

        /// <summary>
        /// 获取请求的文件
        /// </summary>
        public HttpFile[] Files { get; internal set; }

        /// <summary>
        /// 获取Post的内容
        /// </summary>
        public byte[] Body { get; internal set; }

        /// <summary>
        /// 获取请求方法
        /// </summary>
        public HttpMethod HttpMethod { get; internal set; }

        /// <summary>
        /// 获取请求路径
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// 获取请求的Uri
        /// </summary>
        public Uri Url { get; internal set; }

        /// <summary>
        /// 获取监听的本地IP和端口
        /// </summary>
        public EndPoint LocalEndPoint { get; internal set; }

        /// <summary>
        /// 获取远程端的IP和端口
        /// </summary>
        public EndPoint RemoteEndPoint { get; internal set; }

       
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
                if (this.Query.ContainsKey(key))
                {
                    return this.Query[key];
                }
                else
                {
                    return this.Form[key];
                }
            }
        }

        /// <summary>
        /// Http请求信息
        /// </summary>
        internal HttpRequest()
        {
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
        /// 是否为ajax请求
        /// </summary>
        /// <returns></returns>
        public bool IsAjaxRequest()
        {
            return this["X-Requested-With"] == "XMLHttpRequest" || this.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// 是否为event-stream请求
        /// </summary>
        /// <returns></returns>
        public bool IsEventStreamRequest()
        {
            return StringEquals(this.Headers["Accept"], "text/event-stream");
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
            if (string.IsNullOrEmpty(contentType) == false)
            {
                if (Regex.IsMatch(contentType, "multipart/form-data", RegexOptions.IgnoreCase))
                {
                    var match = Regex.Match(contentType, "(?<=boundary=).+");
                    boundary = match.Value;
                    return match.Success;
                }
            }

            boundary = null;
            return false;
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
    }
}

