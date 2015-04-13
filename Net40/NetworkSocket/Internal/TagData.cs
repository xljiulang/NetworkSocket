using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示用户附加数据
    /// </summary>
    internal class TagData : ITag
    {
        /// <summary>
        /// 原始数据字典
        /// </summary>
        private Dictionary<string, object> dic = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

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
        public bool Exist(string key)
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
    }
}
