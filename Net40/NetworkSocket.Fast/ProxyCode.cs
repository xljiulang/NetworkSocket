using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示代理源码
    /// </summary>
    public class ProxyCode
    {
        /// <summary>
        /// 源码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 代理源码
        /// </summary>
        /// <param name="code">源码</param>
        internal ProxyCode(string code)
        {
            this.Code = code;
        }

        /// <summary>
        /// 将源码写入文件 
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void WriteToFile(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            var newCode = this.Code;
            if (File.Exists(filePath) == true)
            {
                // 保留using空间
                var oldCode = File.ReadAllText(filePath);
                newCode = Regex.Replace(oldCode, "public abstract.+", this.Code, RegexOptions.Singleline);
            }
            File.WriteAllText(filePath, newCode);
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Code;
        }
    }
}
