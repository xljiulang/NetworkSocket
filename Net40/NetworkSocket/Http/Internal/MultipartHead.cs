using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表单头
    /// </summary>
    internal class MultipartHead
    {
        /// <summary>
        /// 头数据
        /// </summary>
        private string head;

        /// <summary>
        /// 表单头
        /// </summary>
        /// <param name="head"></param>
        public MultipartHead(string head)
        {
            this.head = head;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetValue(string key)
        {
            var pattern = string.Format("(?<={0}=\").+?(?=\")", key);
            return Regex.Match(head, pattern, RegexOptions.IgnoreCase).Value;
        }

        /// <summary>
        /// 获取名称
        /// </summary>
        public string Name
        {
            get
            {
                return this.GetValue("name");
            }
        }
        /// <summary>
        /// 获取文件名
        /// </summary>
        public string FileName
        {
            get
            {
                return this.GetValue("filename");
            }
        }

        /// <summary>
        /// 获取是否为文件
        /// </summary>
        public bool IsFile
        {
            get
            {
                return Regex.IsMatch(head, "Content-Type", RegexOptions.IgnoreCase);
            }
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
