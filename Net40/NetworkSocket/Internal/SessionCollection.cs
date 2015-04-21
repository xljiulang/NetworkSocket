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
    /// 有效会话对象集合 
    /// 线程安全类型
    /// </summary>   
    /// <typeparam name="T">会话</typeparam>
    [DebuggerDisplay("Count = {dic.Count}")]
    internal class SessionCollection<T> : ICollection<T>, IDisposable where T : SessionBase
    {
        /// <summary>
        /// 线程安全字典
        /// </summary>
        private ConcurrentDictionary<int, T> dic = new ConcurrentDictionary<int, T>();

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
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

        /// <summary>
        /// 添加 
        /// 如果已包含此元素则不会增加记录
        /// </summary>
        /// <param name="session">会话</param>
        /// <returns></returns>
        void ICollection<T>.Add(T session)
        {
            if (session != null)
            {
                var key = session.GetHashCode();
                this.dic.TryAdd(key, session);
            }
        }

        /// <summary>
        /// 清除所有元素
        /// </summary>
        void ICollection<T>.Clear()
        {
            this.dic.Clear();
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="session">会话</param>
        /// <returns></returns>
        bool ICollection<T>.Contains(T session)
        {
            if (session == null)
            {
                return false;
            }
            var key = session.GetHashCode();
            return this.dic.ContainsKey(key);
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取元素数量 
        /// </summary>
        int ICollection<T>.Count
        {
            get
            {
                return this.dic.Count;
            }
        }

        /// <summary>
        /// 是否为只读
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 移除    
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <returns></returns>
        bool ICollection<T>.Remove(T session)
        {
            if (session == null)
            {
                return false;
            }
            var key = session.GetHashCode();
            return this.dic.TryRemove(key, out session);
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        void IDisposable.Dispose()
        {
            var kvs = this.dic.ToArray();
            foreach (var kv in kvs)
            {
                IDisposable disposable = kv.Value;
                disposable.Dispose();
            }
            this.dic.Clear();
        }
    }
}
