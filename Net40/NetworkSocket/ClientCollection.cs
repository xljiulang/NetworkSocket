using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 客户端对象集合 
    /// 线程安全类型
    /// </summary>   
    /// <typeparam name="T">发送数据包协议</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ClientCollection<T> : IEnumerable<IClient<T>> where T : PacketBase
    {
        /// <summary>
        /// 线程安全字典
        /// </summary>
        private ConcurrentDictionary<int, IClient<T>> dic = new ConcurrentDictionary<int, IClient<T>>();

        /// <summary>
        /// 获取客户端对象的数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.dic.Count;
            }
        }

        /// <summary>
        /// 客户端对象集合
        /// </summary>
        internal ClientCollection()
        {
        }

        /// <summary>
        /// 客户端对象
        /// 如果已包含此元素则返回false，同时不会增加记录
        /// </summary>
        /// <param name="client">客户端对象</param>
        /// <returns></returns>
        internal bool Add(IClient<T> client)
        {
            if (client == null)
            {
                return false;
            }
            var key = client.GetHashCode();
            return dic.TryAdd(key, client);
        }

        /// <summary>
        /// 移除客户端对象
        /// 如果客户端对象不存在而返回false
        /// </summary>
        /// <param name="client">客户端对象</param>
        /// <returns></returns>
        internal bool Remove(IClient<T> client)
        {
            if (client == null)
            {
                return false;
            }
            var key = client.GetHashCode();
            return this.dic.TryRemove(key, out client);
        }

        /// <summary>
        /// 清空所有元素
        /// </summary>
        internal void Clear()
        {
            this.dic.Clear();
        }

        /// <summary>
        /// 将对象复制到数组中
        /// </summary>
        /// <returns></returns>
        public IClient<T>[] ToArray()
        {
            return this.dic.ToArray().Select(item => item.Value).ToArray();
        }

        /// <summary>
        /// 将对象复制到列表中
        /// </summary>
        /// <returns></returns>
        public List<IClient<T>> ToList()
        {
            return new List<IClient<T>>(this.dic.ToArray().Select(item => item.Value));
        }

        /// <summary>
        /// 通过使用默认的相等比较器对值进行比较得到集合的差集。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <returns></returns>
        public IEnumerable<IClient<T>> Except(IClient<T> client)
        {
            return this.Except(new[] { client });
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IClient<T>> GetEnumerator()
        {
            var enumerator = this.dic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current.Value;
            }
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
