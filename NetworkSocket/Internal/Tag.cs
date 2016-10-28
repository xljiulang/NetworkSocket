using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示用户附加数据
    /// </summary>   
    internal class Tag : ConcurrentDictionary<string, object>, ITag
    {
        /// <summary>
        /// 获取或设置唯一标识符
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 表示用户附加数据
        /// </summary>
        public Tag()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Set(string key, object value)
        {
            base.AddOrUpdate(key, value, (k, v) => value);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public TagItem Get(string key)
        {
            object value;
            base.TryGetValue(key, out value);
            return new TagItem(value);
        }
    }
}
