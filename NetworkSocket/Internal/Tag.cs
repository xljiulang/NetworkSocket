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
    internal class Tag : ITag
    {
        /// <summary>
        /// 获取或设置唯一标识符
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 获取或设置其它用户数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 返回Data的转换类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T DataCast<T>()
        {
            return (T)this.Data;
        }
    }
}
