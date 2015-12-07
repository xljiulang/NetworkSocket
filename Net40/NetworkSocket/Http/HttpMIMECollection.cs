using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示文件扩展类型集合
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public class HttpMIMECollection : ICollection<HttpMIME>
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// 保存MIME的字典
        /// </summary>
        private Dictionary<string, string> mimes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// 获取元素数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.mimes.Count;
            }
        }

        /// <summary>
        /// 填充一些常用的扩展类型
        /// </summary>
        public void FillBasicMIME()
        {
            this.Add(".js", "text/javascript");
            this.Add(".css", "text/css");

            this.Add(".htm", "text/html");
            this.Add(".html", "text/html");

            this.Add(".xml", "text/xml");
            this.Add(".xhtml", "application/xhtml+xml");

            this.Add(".txt", "text/plain");
            this.Add(".rtf", "application/rtf");
            this.Add(".pdf", "application/pdf");

            this.Add(".doc", "application/msword");
            this.Add(".xls", "application/vnd.ms-excel application/x-excel");
            this.Add(".ppt", "application/vnd.ms-powerpoint");

            this.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            this.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            this.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");

            this.Add(".swf", "application/x-shockwave-flash");

            this.Add(".png", "image/png");
            this.Add(".gif", "image/gif");
            this.Add(".jpg", "image/jpeg");
            this.Add(".jpeg", "image/jpeg");
            this.Add(".ico", "image/x-icon");
            this.Add(".bmp", "image/bmp");

            this.Add(".rar", "application/ocelet-stream");
            this.Add(".zip", "application/x-zip-compressed");

            this.Add(".apk", "application/vnd.android.package-archive");
            this.Add(".ipa", "application/iphone-package-archive");
        }

        /// <summary>
        /// 添加扩展类型
        /// </summary>
        /// <param name="item">扩展类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(HttpMIME item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            this.mimes[item.Extension] = item.ContentType;
        }

        /// <summary>
        /// 添加扩展类型
        /// </summary>
        /// <param name="extension">扩展名</param>
        /// <param name="contentType">内容类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(string extension, string contentType)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentNullException();
            }
            this.mimes[extension] = contentType;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool ICollection<HttpMIME>.Remove(HttpMIME item)
        {
            return item != null && this.mimes.Remove(item.Extension);
        }

        /// <summary>
        /// 清除集合
        /// </summary>
        public void Clear()
        {
            this.mimes.Clear();
        }

        /// <summary>
        /// 获取扩展的类型
        /// </summary>
        /// <param name="extension">扩展名</param>
        /// <returns></returns>
        public string GetContentType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }
            string contentType;
            this.mimes.TryGetValue(extension, out contentType);
            return contentType;
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool ICollection<HttpMIME>.Contains(HttpMIME item)
        {
            return item != null && this.mimes.ContainsKey(item.Extension);
        }

        /// <summary>
        /// 控制到数组
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void ICollection<HttpMIME>.CopyTo(HttpMIME[] array, int arrayIndex)
        {
            var kvArray = new KeyValuePair<string, string>[array.Length];
            ((ICollection<KeyValuePair<string, string>>)this.mimes).CopyTo(kvArray, arrayIndex);
            for (var i = arrayIndex; i < array.Length; i++)
            {
                array[i] = new HttpMIME(kvArray[i].Key, kvArray[i].Value);
            }
        }

        /// <summary>
        /// 是否为只读
        /// </summary>
        bool ICollection<HttpMIME>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<HttpMIME> GetEnumerator()
        {
            foreach (var item in this.mimes)
            {
                yield return new HttpMIME(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// 调试视图
        /// </summary>
        private class DebugView
        {
            /// <summary>
            /// 查看的对象
            /// </summary>
            private HttpMIMECollection view;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="view">查看的对象</param>
            public DebugView(HttpMIMECollection view)
            {
                this.view = view;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public HttpMIME[] Values
            {
                get
                {
                    return view.ToArray();
                }
            }
        }
    }
}
