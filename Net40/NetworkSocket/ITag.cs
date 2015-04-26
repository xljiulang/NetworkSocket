using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 用户附加数据接口
    /// </summary>
    public interface ITag
    {
        /// <summary>
        /// 设置用户数据
        /// </summary>
        /// <param name="key">键(不区分大小写)</param>
        /// <param name="value">用户数据</param>
        void Set(string key, object value);

        /// <summary>
        /// 是否存在键
        /// </summary>
        /// <param name="key">键(不区分大小写)</param>
        /// <returns></returns>
        bool IsExist(string key);

        /// <summary>
        /// 尝试获取值
        /// 获取失败则返回类型的默认值
        /// </summary>       
        /// <param name="key">键(不区分大小写)</param>
        /// <returns></returns>
        object TryGet(string key);

        /// <summary>
        /// 尝试获取值
        /// 获取失败则返回类型的默认值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键(不区分大小写)</param>
        /// <returns></returns>
        T TryGet<T>(string key);

        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键(不区分大小写)</param>
        /// <param name="defaultValue">获取失败返回的默认值</param>
        /// <returns></returns>
        T TryGet<T>(string key, T defaultValue);

        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="key">键(不区分大小写)</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        bool TryGet(string key, out object value);

        /// <summary>
        /// 删除用户数据
        /// </summary>
        /// <param name="key">键(不区分大小写)</param>
        bool Remove(string key);

        /// <summary>
        /// 清除所有用户数据
        /// </summary>
        void Clear();
    }
}
