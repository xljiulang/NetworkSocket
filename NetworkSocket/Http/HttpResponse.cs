using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http回复对象
    /// </summary>
    public class HttpResponse : IWrapper
    {
        /// <summary>
        /// 会话对象
        /// </summary>
        private ISession session;

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
        public EndPoint RemoteEndPoint
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
        public HttpResponse(ISession session)
        {
            this.session = session;

            this.Charset = Encoding.UTF8;
            this.Status = 200;
            this.StatusDescription = "OK";

            this.Headers = new HttpHeader();
            this.ContentType = "text/html";
        }

        /// <summary>
        /// 输出不包含Content-Length的头数据        
        /// </summary>
        /// <returns></returns>
        public bool WriteHeader()
        {
            return this.WriteHeader(-1, gzip: false);
        }

        /// <summary>
        /// 输出头数据
        /// </summary>
        /// <param name="contentLength">内容长度</param>
        /// <param name="gzip">gzip模式</param>
        /// <returns></returns>
        public bool WriteHeader(int contentLength, bool gzip = false)
        {
            var headerByes = this.GenerateHeader(contentLength, gzip);
            return this.TrySend(headerByes);
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="buffer">内容</param>      
        /// <returns></returns>
        public bool WriteContent(byte[] buffer)
        {
            return this.TrySend(buffer);
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="buffer">内容</param>
        /// <returns></returns>
        public bool WriteContent(ArraySegment<byte> buffer)
        {
            return this.TrySend(buffer);
        }


        /// <summary>
        /// 输出回复内容
        /// 自动设置回复头的Content-Length
        /// </summary>      
        /// <param name="content">内容</param>
        /// <param name="gzip">gzip模式</param>
        public bool WriteResponse(string content, bool gzip = false)
        {
            if (content == null)
            {
                content = string.Empty;
            }

            var contentBytes = this.Charset.GetBytes(content);
            if (gzip == true)
            {
                contentBytes = Compression.GZipCompress(contentBytes);
            }

            var headerBytes = this.GenerateHeader(contentBytes.Length, gzip);
            var buffer = this.ConcatBuffer(headerBytes, contentBytes);
            return this.TrySend(buffer);
        }

        /// <summary>
        /// 生成头部数据
        /// </summary>
        /// <param name="contentLength">内容长度</param>
        /// <param name="gzip">gzip模式</param>
        /// <returns></returns>
        private byte[] GenerateHeader(int contentLength, bool gzip)
        {
            var header = new ResponseHeader(this.Status, this.StatusDescription);
            if (this.Charset == null)
            {
                header.Add("Content-Type", this.ContentType);
            }
            else
            {
                var contenType = string.Format("{0}; charset={1}", this.ContentType, this.Charset.WebName);
                header.Add("Content-Type", contenType);
            }

            if (contentLength >= 0)
            {
                header.Add("Content-Length", contentLength);
            }

            if (gzip == true)
            {
                header.Add("Content-Encoding", "gzip");
            }

            var assemblyName = typeof(HttpMiddleware).Assembly.GetName();
            header.Add("Date", DateTime.Now.ToUniversalTime().ToString("r"));
            header.Add("Server", assemblyName.Name + assemblyName.Version.ToString());

            foreach (var key in this.Headers.AllKeys)
            {
                header.Add(key, this.Headers[key]);
            }

            return header.ToByteArray();
        }


        /// <summary>
        /// 连接buffer
        /// </summary>
        /// <param name="buffer1"></param>
        /// <param name="buffer2"></param>
        /// <returns></returns>
        private byte[] ConcatBuffer(byte[] buffer1, byte[] buffer2)
        {
            var length = buffer1.Length + buffer2.Length;
            var buffer = new byte[length];
            Buffer.BlockCopy(buffer1, 0, buffer, 0, buffer1.Length);
            Buffer.BlockCopy(buffer2, 0, buffer, buffer1.Length, buffer2.Length);
            return buffer;
        }


        /// <summary>
        /// 尝试发送数据到客户端
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private bool TrySend(ArraySegment<byte> range)
        {
            try
            {
                return this.session.Send(range) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试发送数据到客户端
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private bool TrySend(byte[] buffer)
        {
            try
            {
                return this.session.Send(buffer) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 主动关闭连接
        /// </summary>
        public void End()
        {
            this.session.Close();
        }

        /// <summary>
        /// 还原到包装前
        /// </summary>
        /// <returns></returns>
        ISession IWrapper.UnWrap()
        {
            return this.session;
        }
    }
}
