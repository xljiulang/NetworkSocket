using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义会话附加数据的接口
    /// </summary>
    public interface ITag
    {
        /// <summary>
        /// 获取或设置唯一标识符
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// 获取或设置其它用户数据
        /// </summary>
        object Data { get; set; }

        /// <summary>
        /// 返回Data的转换类型
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        T DataCast<T>();
    }
}
