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
        /// 参数
        /// </summary>
        private string parameters;

        /// <summary>
        /// 表示可通过键或索引访问的集合
        /// </summary>
        public HttpNameValueCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// 表示可通过键或索引访问的集合
        /// </summary>
        /// <param name="parameters">参数</param>
        public HttpNameValueCollection(string parameters)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            this.parameters = parameters;
            this.AddAsKeyValue(parameters);
        }

        /// <summary>
        /// 填充集合
        /// </summary>
        /// <param name="parameters"></param>
        private void AddAsKeyValue(string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters) == true)
            {
                return;
            }

            var kvs = parameters.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in kvs)
            {
                var kv = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 2)
                {
                    base.Add(kv.FirstOrDefault(), kv.LastOrDefault());
                }
            }
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.parameters;
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
