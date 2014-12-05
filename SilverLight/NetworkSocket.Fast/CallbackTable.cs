using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 回调信息维护表
    /// 所有方法都是线程安全
    /// </summary>
    internal static class CallbackTable
    {
        /// <summary>
        /// 锁
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// 回调信息字典
        /// </summary>
        private static Dictionary<int, Action<byte[]>> dic = new Dictionary<int, Action<byte[]>>();

        /// <summary>
        /// 添加回调信息记录       
        /// </summary>
        /// <param name="hashCode">哈希码</param>
        /// <param name="callBack">回调</param>       
        /// <returns></returns>
        public static void Add(int hashCode, Action<byte[]> callBack)
        {
            lock (syncRoot)
            {
                dic.Add(hashCode, callBack);
            }
        }

        /// <summary>
        /// 通过哈希码查找并移除匹配记录
        /// 返回匹配记录的回调信息
        /// 如果没有匹配项，返回null
        /// </summary>
        /// <param name="hashCode">匹配的哈希码</param>
        /// <returns></returns>
        public static Action<byte[]> Take(int hashCode)
        {
            lock (syncRoot)
            {
                Action<byte[]> callBack;
                if (dic.TryGetValue(hashCode, out callBack))
                {
                    dic.Remove(hashCode);                   
                }
                return callBack;
            }
        }
    }
}
