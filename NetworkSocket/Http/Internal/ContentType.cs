using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                    return this.content;
                }
                return Regex.Match(this.content, @"^.+?(?=;|$)").Value.Trim();
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
            if (this.content == null)
            {
                return false;
            }

            var option = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return Regex.IsMatch(this.content, string.Format(@"^{0}(?=;|$)", value), option);
        }

        /// <summary>
        /// 获取额外内容
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public bool TryGetExtend(string key, out string value)
        {
            var match = Regex.Match(this.content, string.Format(@"(?<={0}=).+?(?=;|$)", key), RegexOptions.IgnoreCase);
            value = match.Value;
            return match.Success;
        }
    }
}
