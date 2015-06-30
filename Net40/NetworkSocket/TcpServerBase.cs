
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;


namespace NetworkSocket
{
    /// <summary>
    /// Tcp服务端抽象类
    /// 提供对客户端池的初始化、自动回收重用、在线客户端列表维护功能
    /// 提供客户端连接、断开通知功能
    /// 所有Tcp服务端都派生于此类
    /// </summary>
    /// <typeparam name="T">会话类型</typeparam>   
    [DebuggerDisplay("IsListening = {IsListening}")]
    public abstract class TcpServerBase<T> : ITcpServer<T> where T : SessionBase
    {
        /// <summary>
        /// 所有会话的数量
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int totalSessionCount;

        /// <summary>
        /// 接受会话失败的次数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int acceptFailureTimes;

        /// <summary>
        /// 服务socket
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private volatile Socket listenSocket;

        /// <summary>
        /// 请求参数
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();

        /// <summary>
        /// 空闲客户端池
        /// </summary>        
        private FreeSessionStack<T> freeSessionStack = new FreeSessionStack<T>();



        /// <summary>
        /// 获取当前所有会话对象   
        /// </summary>
        public IEnumerable<T> AllSessions { get; private set; }

        /// <summary>
        /// 获取所监听的本地IP和端口
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// 获取服务是否已处在监听中
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// 获取额外信息
        /// </summary>
        public ServerExtraState ExtraState { get; private set; }

        /// <summary>
        /// Tcp服务端抽象类
        /// </summary> 
        public TcpServerBase()
        {
            this.AllSessions = new SessionCollection<T>();
            this.ExtraState = new ServerExtraState(
                () => this.freeSessionStack.Count,
                () => this.totalSessionCount,
                () => this.acceptFailureTimes);
        }

        /// <summary>
        /// 开始启动监听
        /// 如果IsListening为true，将不产生任何作用
        /// </summary>
        /// <param name="port">端口</param>
        /// <exception cref="SocketException"></exception>
        public void StartListen(int port)
        {
            this.StartListen(new IPEndPoint(IPAddress.Any, port));
        }

        /// <summary>
        /// 开始启动监听
        /// 如果IsListening为true，将不产生任何作用
        /// </summary>
        /// <param name="localEndPoint">要监听的本地IP和端口</param>    
        /// <exception cref="SocketException"></exception>
        public void StartListen(IPEndPoint localEndPoint)
        {
            if (this.IsListening)
            {
                return;
            }

            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(localEndPoint);
            this.listenSocket.Listen(100);

            this.acceptArg = new SocketAsyncEventArgs();
            this.acceptArg.Completed += (sender, e) => this.AcceptArgCompleted(e);
            this.AcceptSession(this.acceptArg);

            this.LocalEndPoint = localEndPoint;
            this.IsListening = true;
        }

        /// <summary>
        /// 开始一次接受连接请求操作
        /// </summary>
        /// <param name="arg">连接参数</param>     
        private void AcceptSession(SocketAsyncEventArgs arg)
        {
            if (this.listenSocket != null)
            {
                arg.AcceptSocket = null;
                if (this.listenSocket.AcceptAsync(arg) == false)
                {
                    this.AcceptArgCompleted(arg);
                }
            }
        }

        /// <summary>
        /// 连接请求IO完成
        /// </summary>
        /// <param name="arg">连接参数</param>
        private void AcceptArgCompleted(SocketAsyncEventArgs arg)
        {
            var socket = arg.AcceptSocket;

            if (arg.SocketError == SocketError.Success)
            {
                var session = this.TakeOrCreateSession();
                if (session != null)
                {
                    this.InitSession(session, socket);
                }
                else
                {
                    socket.Close();
                    socket.Dispose();
                }
            }
            else
            {
                Interlocked.Increment(ref this.acceptFailureTimes);
                var innerException = new SocketException((int)arg.SocketError);
                this.OnException(this, new SessionAcceptExcetion(innerException));
            }

            // 处理后继续接收
            this.AcceptSession(arg);
        }


        /// <summary>
        /// 从空闲池中取出或创建Session会话对象
        /// </summary>
        /// <returns></returns>
        private T TakeOrCreateSession()
        {
            // 从池中取出SocketAsync
            var session = this.freeSessionStack.Take();
            if (session != null)
            {
                return session;
            }

            try
            {
                session = this.OnCreateSession();
                if (session != null)
                {
                    Interlocked.Increment(ref this.totalSessionCount);
                }
            }
            catch (SessionCreateException ex)
            {
                this.OnException(this, ex);
            }
            catch (Exception ex)
            {
                this.OnException(this, new SessionCreateException(ex));
            }
            return session;
        }

        /// <summary>
        /// 初始化Session会话
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="socket">要绑定的socket</param>
        private void InitSession(T session, Socket socket)
        {
            // 绑定处理委托
            session.ReceiveHandler = (buffer) => this.OnReceive(session, buffer);
            session.DisconnectHandler = () => this.RecyceSession(session);
            session.CloseHandler = () => this.RecyceSession(session);

            // SocketAsync与socket绑定    
            session.Bind(socket);
            // 添加到活动列表
            (this.AllSessions as ICollection<T>).Add(session);
            // 通知已连接
            this.OnConnect(session);
            // 开始接收数据
            session.TryReceive();
        }

        /// <summary>
        /// 回收复用会话对象
        /// 关闭会话并通知连接断开
        /// </summary>
        /// <param name="session">会话对象</param>
        private void RecyceSession(T session)
        {
            if ((this.AllSessions as ICollection<T>).Remove(session) == true)
            {
                this.OnDisconnect(session);
                session.Close();
                session.TagData.Clear();
                this.freeSessionStack.Add(session);
            }
        }

        /// <summary>
        /// 创建新的会话对象
        /// </summary>
        /// <returns></returns>
        protected abstract T OnCreateSession();

        /// <summary>
        /// 创建会话或解析请求数据产生异常触发
        /// </summary>
        /// <param name="sender">异常产生者</param>
        /// <param name="exception">异常</param>
        protected virtual void OnException(object sender, Exception exception)
        {
        }

        /// <summary>
        /// 当接收到会话对象的数据时，将触发此方法             
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="buffer">接收到的历史数据</param>
        /// <returns></returns>
        protected abstract void OnReceive(T session, ReceiveBuffer buffer);


        /// <summary>
        /// 当会话断开连接时，将触发此方法
        /// </summary>
        /// <param name="session">会话对象</param>     
        protected virtual void OnDisconnect(T session)
        {
        }

        /// <summary>
        /// 当会话连接时，将触发此方法
        /// </summary>
        /// <param name="session">会话对象</param>
        protected virtual void OnConnect(T session)
        {
        }

        #region IDisposable
        /// <summary>
        /// 获取对象是否已释放
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 关闭和释放所有相关资源
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed == false)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
            this.IsDisposed = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TcpServerBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.listenSocket != null)
            {
                try
                {
                    this.listenSocket.Dispose();
                }
                finally
                {
                    this.listenSocket = null;
                }
            }

            (this.AllSessions as IDisposable).Dispose();
            this.freeSessionStack.Dispose();
            this.acceptArg.Dispose();

            if (disposing)
            {
                this.AllSessions = null;
                this.acceptArg = null;
                this.freeSessionStack = null;
                this.LocalEndPoint = null;
                this.IsListening = false;
            }
        }
        #endregion
    }
}
