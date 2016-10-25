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
    /// 表示Tcp会话对象集合 
    /// 线程安全类型
    /// </summary>   
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(SessionCollectionDebugView))]
    internal class TcpSessionCollection : ICollection<TcpSessionBase>, ISessionManager, IDisposable
    {
        /// <summary>
        /// 线程安全字典
        /// </summary>
        private readonly ConcurrentDictionary<int, TcpSessionBase> dic = new ConcurrentDictionary<int, TcpSessionBase>();

        /// <summary>
        /// 获取元素数量 
        /// </summary>
        public int Count
        {
            get
            {
                return this.dic.Count;
            }
        }

        /// <summary>
        /// 是否为只读
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 添加 
        /// 如果已包含此元素则不会增加记录
        /// </summary>
        /// <param name="session">会话</param>
        /// <returns></returns>
        public void Add(TcpSessionBase session)
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
        public void Clear()
        {
            this.dic.Clear();
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="session">会话</param>
        /// <returns></returns>
        public bool Contains(TcpSessionBase session)
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
        public void CopyTo(TcpSessionBase[] array, int arrayIndex)
        {
            var index = 0;
            var kvs = this.dic.ToArray();

            for (var i = arrayIndex; i < array.Length; i++)
            {
                if (index == kvs.Length)
                {
                    break;
                }
                else
                {
                    array[i] = kvs[index].Value;
                    index++;
                }
            }
        }

        /// <summary>
        /// 移除    
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <returns></returns>
        public bool Remove(TcpSessionBase session)
        {
            if (session == null)
            {
                return false;
            }
            var key = session.GetHashCode();
            return this.dic.TryRemove(key, out session);
        }

        /// <summary>
        /// 获取会话的包装对象
        /// </summary>
        /// <typeparam name="TWapper">包装类型</typeparam>
        /// <returns></returns>
        public IEnumerable<TWapper> FilterWrappers<TWapper>() where TWapper : class, IWrapper
        {
            return this.Select(item => item.Wrapper).OfType<TWapper>();
        }

        /// <summary>
        /// 获取过滤了协议类型的会话对象
        /// </summary>
        /// <param name="protocol">协议类型</param>
        /// <returns></returns>
        public IEnumerable<ISession> FilterProtocol(Protocol protocol)
        {
            return this.Where(item => item.Protocol == protocol);
        }


        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TcpSessionBase> GetEnumerator()
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
        /// 释放资源
        /// </summary>
        public void Dispose()
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


    /// <summary>
    /// 调试视图
    /// </summary>
    internal class SessionCollectionDebugView
    {
        /// <summary>
        /// 查看的对象
        /// </summary>
        private TcpSessionCollection view;

        /// <summary>
        /// 调试视图
        /// </summary>
        /// <param name="view">查看的对象</param>
        public SessionCollectionDebugView(TcpSessionCollection view)
        {
            this.view = view;
        }

        /// <summary>
        /// 查看的内容
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TcpSessionBase[] Values
        {
            get
            {
                return this.view.ToArray();
            }
        }
    }
}
