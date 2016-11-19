using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表单头
    /// </summary>
    internal class MultipartHead
    {
        /// <summary>
        /// 头数据内容
        /// </summary>
        private readonly string content;

        /// <summary>
        /// 获取名称
        /// </summary>
        public string Name
        {
            get
            {
                string value = null;
                this.TryGet("name", out value);
                return value;
            }
        }

        /// <summary>
        /// 表单头
        /// </summary>
        /// <param name="content">内容</param>
        public MultipartHead(string content)
        {
            this.content = content;
        }

        /// <summary>
        /// 尝试获取文件名
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public bool TryGetFileName(out string fileName)
        {
            return this.TryGet("filename", out fileName);
        }

        /// <summary>
        /// 获取额外内容
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private bool TryGet(string key, out string value)
        {
            value = null;
            key = key + "=\"";
            var keyIndex = this.content.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return false;
            }

            var valueIndex = keyIndex + key.Length;
            if (valueIndex >= this.content.Length)
            {
                return false;
            }

            var length = this.content.IndexOf('"', valueIndex) - valueIndex;
            if (length < 0)
            {
                length = this.content.Length - valueIndex;
            }
            value = this.content.Substring(valueIndex, length);
            return true;
        }


        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
