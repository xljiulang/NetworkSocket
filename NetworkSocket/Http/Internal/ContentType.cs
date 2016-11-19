using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示请求的ContentType
    /// </summary>
    internal class ContentType
    {
        /// <summary>
        /// 内容
        /// </summary>
        private readonly string content;

        /// <summary>
        /// 请求的ContentType
        /// </summary>
        /// <param name="request">请求</param>
        public ContentType(HttpRequest request)
        {
            this.content = request.Headers["Content-Type"];
        }

        /// <summary>
        /// 获取ContentType的值
        /// </summary>
        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(this.content))
                {
                    return string.Empty;
                }

                var length = this.content.IndexOf(';');
                if (length < 0)
                {
                    return content;
                }
                return content.Substring(0, length);
            }
        }

        /// <summary>
        /// ContentType的值是否与value相等
        /// </summary>
        /// <param name="value">目标值</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public bool IsMatch(string value, bool ignoreCase = true)
        {
            if (ignoreCase == true)
            {
                return string.Equals(this.Value, value, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return this.Value == null;
            }
        }

        /// <summary>
        /// 获取额外内容
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool TryGetExtend(string key, out string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException();
            }

            value = null;
            key = key + "=";
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

            var length = this.content.IndexOf(';', valueIndex) - valueIndex;
            if (length < 0)
            {
                length = this.content.Length - valueIndex;
            }
            value = this.content.Substring(valueIndex, length);
            return true;
        }

        /// <summary>
        /// 获取额外内容
        /// 如果获取失败则返回默认值 
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值 </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public string TryGetExtend(string key, string defaultValue)
        {
            string value = null;
            if (this.TryGetExtend(key, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
