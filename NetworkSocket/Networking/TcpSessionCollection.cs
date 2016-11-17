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
    internal class TcpSessionCollection : ISessionManager, IEnumerable<TcpSessionBase>, IDisposable
    {
        /// <summary>
        /// 线程安全字典
        /// </summary>
        private readonly ConcurrentDictionary<Guid, TcpSessionBase> sessions = new ConcurrentDictionary<Guid, TcpSessionBase>();

        /// <summary>
        /// 获取元素数量 
        /// </summary>
        public int Count
        {
            get
            {
                return this.sessions.Count;
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
                this.sessions.TryAdd(session.ID, session);
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
            return this.sessions.TryRemove(session.ID, out session);
        }

        /// <summary>
        /// 获取会话的包装对象
        /// </summary>
        /// <typeparam name="TWapper">包装类型</typeparam>
        /// <returns></returns>
        IEnumerable<TWapper> ISessionManager.FilterWrappers<TWapper>()
        {
            return this.Select(item => item.Wrapper).OfType<TWapper>();
        }

        /// <summary>
        /// 获取过滤了协议类型的会话对象
        /// </summary>
        /// <param name="protocol">协议类型</param>
        /// <returns></returns>
        IEnumerable<ISession> ISessionManager.FilterProtocol(Protocol protocol)
        {
            return this.Where(item => item.Protocol == protocol);
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TcpSessionBase> GetEnumerator()
        {
            return this.sessions.Values.GetEnumerator();
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
            foreach (var item in this)
            {
                item.Dispose();
            }
            this.sessions.Clear();
        }


        /// <summary>
        /// 调试视图
        /// </summary>
        private class SessionCollectionDebugView
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

}
