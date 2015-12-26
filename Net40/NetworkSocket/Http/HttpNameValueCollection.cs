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
        /// 从参数字符串生成键或索引集合
        /// 如果参数未解码过，则将被解码再处理
        /// </summary>
        /// <param name="parameters">http请求参数</param>
        /// <param name="decoded">参数是否已解码过</param>
        /// <returns></returns>
        public static HttpNameValueCollection Parse(string parameters, bool decoded)
        {
            var collection = new HttpNameValueCollection();
            if (string.IsNullOrWhiteSpace(parameters) == true)
            {
                return collection;
            }

            if (decoded == false)
            {
                parameters = HttpUtility.UrlDecode(parameters, Encoding.UTF8);
            }

            var keyValues = parameters.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in keyValues)
            {
                var kv = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 2)
                {
                    collection.Add(kv[0], kv[1]);
                }
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
