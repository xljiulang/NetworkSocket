using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示用户附加数据
    /// </summary>
    [DebuggerDisplay("Count = {dic.Count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    internal class TagData : ITag
    {
        /// <summary>
        /// 原始数据字典
        /// </summary>
        private Dictionary<string, object> dic = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取所有key
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                return this.dic.Keys;
            }
        }

        /// <summary>
        /// 所有键值
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> KeyValues
        {
            get
            {
                return this.dic;
            }
        }

        /// <summary>
        /// 设置用户数据
        /// </summary>
        /// <param name="key">键(不区分大小写)</param>
        /// <param name="value">用户数据</param>
        public void Set(string key, object value)
        {
            this.dic[key] = value;
        }

        /// <summary>
        /// 是否存在键
        /// </summary>
        /// <param name="key">键(不区分大小写)</param>
        /// <returns></returns>
        public bool IsExist(string key)
        {
            return this.dic.ContainsKey(key);
        }

        /// <summary>
        /// 尝试获取值
        /// 获取失败则返回类型的默认值
        /// </summary>       
        /// <param name="key">键(不区分大小写)</param>
        /// <returns></returns>
        public object TryGet(string key)
        {
            object value;
            this.dic.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// 尝试获取值
        /// 获取失败则返回类型的默认值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public T TryGet<T>(string key)
        {
            object value;
            if (this.dic.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return default(T);
        }

        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">获取失败返回的默认值</param>
        /// <returns></returns>
        public T TryGet<T>(string key, T defaultValue)
        {
            object value;
            if (this.dic.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 删除用户数据
        /// </summary>
        /// <param name="key">键(不区分大小写)</param>
        public void Remove(string key)
        {
            if (this.IsExist(key))
            {
                this.dic.Remove(key);
            }
        }

        /// <summary>
        /// 清除所有用户数据
        /// </summary>
        public void Clear()
        {
            this.dic.Clear();
        }


        /// <summary>
        /// 调试视图
        /// </summary>
        private class DebugView
        {
            /// <summary>
            /// 查看的对象
            /// </summary>
            private TagData view;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="view">查看的对象</param>
            public DebugView(TagData view)
            {
                this.view = view;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<string, object>[] Values
            {
                get
                {
                    return this.view.KeyValues.ToArray();
                }
            }
        }
    }
}
