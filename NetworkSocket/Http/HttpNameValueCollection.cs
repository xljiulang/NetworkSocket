using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示可通过键或索引访问的集合
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(HttpNameValueCollectionView))]
    public class HttpNameValueCollection : NameValueCollection
    {
        /// <summary>
        /// 表示可通过键或索引访问的集合
        /// </summary>
        public HttpNameValueCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// 返回是否包含键
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return this.AllKeys.Any(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 从参数字符串生成键或索引集合
        /// </summary>
        /// <param name="parameters">http请求原始参数</param>
        /// <returns></returns>
        public static HttpNameValueCollection Parse(string parameters)
        {
            var collection = new HttpNameValueCollection();
            if (string.IsNullOrWhiteSpace(parameters) == true)
            {
                return collection;
            }

            var keyValues = parameters.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in keyValues)
            {
                var kv = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                var key = HttpUtility.UrlDecode(kv[0], Encoding.UTF8);
                var value = kv.Length == 2 ? HttpUtility.UrlDecode(kv[1], Encoding.UTF8) : null;
                collection.Add(key, value);
            }
            return collection;
        }

        /// <summary>
        /// 调试视图
        /// </summary>
        private class HttpNameValueCollectionView
        {
            /// <summary>
            /// 查看的对象
            /// </summary>
            private HttpNameValueCollection view;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="view">查看的对象</param>
            public HttpNameValueCollectionView(HttpNameValueCollection view)
            {
                this.view = view;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public List<string> Values
            {
                get
                {
                    var list = new List<string>();
                    foreach (string key in view.Keys)
                    {
                        list.Add(string.Format("{0}: {1}", key, view[key]));
                    }
                    return list;
                }
            }
        }
    }
}
