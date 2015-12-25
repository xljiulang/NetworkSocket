using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示文件扩展类型
    /// </summary>    
    public sealed class HttpMIME
    {
        /// <summary>
        /// 获取扩展名
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// 获取类型
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// 文件扩展类型
        /// </summary>
        /// <param name="extension">扩展名，比如.txt</param>
        /// <param name="contentType">类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public HttpMIME(string extension, string contentType)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentNullException("extension");
            }
            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentNullException("contentType");
            }
            if (Path.GetExtension(extension) != extension)
            {
                throw new ArgumentException("extension");
            }
            this.Extension = extension;
            this.ContentType = contentType;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Extension + " " + this.ContentType;
        }      
    }
}
