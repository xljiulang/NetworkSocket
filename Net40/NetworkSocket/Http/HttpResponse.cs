using NetworkSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// Http回复
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// 获取会话对象
        /// </summary>
        public SessionBase Session { get; private set; }

        /// <summary>
        /// 获取或设置Http状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 获取或设置编码
        /// </summary>
        public Encoding Charset { get; set; }

        /// <summary>
        /// 获取或设置内容类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 表示http回复
        /// </summary>
        /// <param name="session">会话</param>
        public HttpResponse(SessionBase session)
        {
            this.Session = session;
            this.Status = 200;
            this.Charset = Encoding.UTF8;
            this.ContentType = "text/html";
        }

        /// <summary>
        /// 写入二进制内容并关闭连接       
        /// </summary>
        /// <param name="bytes">二进制内容</param>
        private void Write(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                if (this.Session.IsConnected)
                {
                    this.Session.Send(new ByteRange(bytes));
                }
            }
            this.Session.Close();
        }

        /// <summary>
        /// 输出文本内容
        /// </summary>      
        /// <param name="content">内容</param>
        public void Write(string content)
        {
            if (content == null)
            {
                content = string.Empty;
            }

            var contentBytes = this.Charset.GetBytes(content);
            var header = new StringBuilder()
                .AppendFormat("HTTP/1.1 {0}", this.Status == 200 ? "200 OK" : this.Status.ToString()).AppendLine()
                .AppendFormat("Content-Type: {0}; charset={1}", this.ContentType, this.Charset.WebName).AppendLine()
                .AppendFormat("Content-Length: {0}", contentBytes.Length).AppendLine()
                .AppendLine()
                .ToString();

            var headerByes = this.Charset.GetBytes(header);
            var bytes = new byte[headerByes.Length + contentBytes.Length];
            headerByes.CopyTo(bytes, 0);
            contentBytes.CopyTo(bytes, headerByes.Length);

            this.Write(bytes);
        }
    }
}
