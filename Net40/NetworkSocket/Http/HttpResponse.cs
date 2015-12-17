using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http回复对象
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// 会话对象
        /// </summary>
        private HttpSession session;

        /// <summary>
        /// 是否已写头信息
        /// </summary>
        private bool wroteHeader = false;


        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.session.IsConnected;
            }
        }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return this.session.RemoteEndPoint;
            }
        }


        /// <summary>
        /// 获取或设置Http状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 获取或设置输出的 HTTP 状态字符串
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 获取或设置内容体的编码
        /// </summary>
        public Encoding Charset { get; set; }

        /// <summary>
        /// 获取回复头信息
        /// </summary>
        public HttpHeader Headers { get; private set; }

        /// <summary>
        /// 获取或设置Header的内容类型
        /// </summary>
        public string ContentType
        {
            get
            {
                return this.Headers["Content-Type"];
            }
            set
            {
                this.Headers["Content-Type"] = value;
            }
        }

        /// <summary>
        /// 获取或设置Header的内容描述
        /// </summary>
        public string ContentDisposition
        {
            get
            {
                return this.Headers["Content-Disposition"];
            }
            set
            {
                this.Headers["Content-Disposition"] = value;
            }
        }


        /// <summary>
        /// 表示http回复
        /// </summary>
        /// <param name="session">会话</param>
        public HttpResponse(HttpSession session)
        {
            this.session = session;

            this.Charset = Encoding.UTF8;
            this.Status = 200;
            this.StatusDescription = "OK";

            this.Headers = new HttpHeader();
            this.ContentType = "text/html";
        }

        /// <summary>
        /// 忽略的key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool IsIgnoreKey(string key)
        {
            var keys = new[] { "Content-Type", "Content-Length", "Date", "Server" };
            return keys.Any(item => string.Equals(key, item, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 获取内容类型
        /// </summary>
        /// <returns></returns>
        private string GenerateContentType()
        {
            if (this.Charset == null)
            {
                return string.Format("Content-Type: {0}", this.ContentType);
            }
            return string.Format("Content-Type: {0}; charset={1}", this.ContentType, this.Charset.WebName);
        }

        /// <summary>
        /// 生成头部数据
        /// </summary>
        /// <param name="contentLength">内容长度</param>
        /// <returns></returns>
        private byte[] GetHeaderBytes(int contentLength)
        {
            var header = new StringBuilder()
                   .AppendFormat("HTTP/1.1 {0} {1}", this.Status, this.StatusDescription).AppendLine()
                   .AppendLine(this.GenerateContentType());

            if (contentLength > -1)
            {
                header.AppendFormat("Content-Length: {0}", contentLength).AppendLine();
            }

            header
                .AppendFormat("Date: {0}", DateTime.Now.ToUniversalTime().ToString("r")).AppendLine()
                .AppendLine("Server: NetworkSocket.HttpServer");

            var keys = this.Headers.AllKeys.Where(item => IsIgnoreKey(item) == false).ToArray();
            foreach (var key in keys)
            {
                var value = this.Headers[key];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    header.AppendFormat("{0}: {1}", key, value).AppendLine();
                }
            }
            return Encoding.ASCII.GetBytes(header.AppendLine().ToString());
        }

        /// <summary>
        /// 输出头数据
        /// </summary>
        /// <returns></returns>
        public bool WriteHeader()
        {
            return this.WriteHeader(-1);
        }

        /// <summary>
        /// 输出头数据
        /// </summary>
        /// <param name="contentLength">内容长度</param>
        /// <returns></returns>
        public bool WriteHeader(int contentLength)
        {
            if (this.wroteHeader == false)
            {
                this.wroteHeader = true;
                var headerByes = this.GetHeaderBytes(contentLength);
                return this.TrySend(new ByteRange(headerByes));
            }
            return false;
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="range">内容</param>
        /// <returns></returns>
        public bool WriteContent(ByteRange range)
        {
            return this.TrySend(range);
        }

        /// <summary>
        /// 输出文本内容
        /// </summary>      
        /// <param name="content">内容</param>
        public bool Write(string content)
        {
            if (content == null)
            {
                content = string.Empty;
            }
            var buffer = new ByteBuilder(Endians.Little);
            var contentBytes = this.Charset.GetBytes(content);

            if (this.wroteHeader == false)
            {
                this.wroteHeader = true;
                var headerByes = this.GetHeaderBytes(contentBytes.Length);
                buffer.Add(headerByes);
            }

            buffer.Add(contentBytes);
            return this.TrySend(buffer.ToByteRange());
        }


        /// <summary>
        /// 尝试发送数据到客户端
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private bool TrySend(ByteRange range)
        {
            if (range == null)
            {
                return false;
            }

            try
            {
                this.session.Send(range);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取封装的Http会话对象
        /// </summary>
        /// <returns></returns>
        public HttpSession GetSession()
        {
            return this.session;
        }

        /// <summary>
        /// 主动关闭连接
        /// </summary>
        public void End()
        {
            this.session.Close();
        }
    }
}
