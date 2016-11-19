using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http文件
    /// </summary>
    public sealed class HttpFile
    {
        /// <summary>
        /// 获取名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取文件名
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 获取文件数据
        /// </summary>
        public byte[] Stream { get; private set; }

        /// <summary>
        /// http文件
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="fileName">文件路径</param>
        /// <param name="stream">数据流</param>
        internal HttpFile(string name, string fileName, byte[] stream)
        {
            this.Name = name;
            this.FileName = fileName;
            this.Stream = stream;
        }

        /// <summary>
        /// 保存到本地文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SaveAs(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException();
            }
            File.WriteAllBytes(path, this.Stream);
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FileName;
        }
    }
}
