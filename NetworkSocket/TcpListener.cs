using NetworkSocket.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 表示Tcp监听服务
    /// </summary>
    public class TcpListener : IListener
    {
        /// <summary>
        /// 用于监听的socket
        /// </summary>
        private volatile Socket listenSocket;

        /// <summary>
        /// 接受参数
        /// </summary>
        private SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();



        /// <summary>
        /// 已回收的会话对象
        /// </summary>        
        private TcpSessionQueue freeSessions = new TcpSessionQueue();

        /// <summary>
        /// 所有工作中的会话对象
        /// </summary>
        private TcpSessionCollection workSessions = new TcpSessionCollection();

        /// <summary>
        /// 所有中间件
        /// </summary>
        private LinkedList<IMiddleware> middlewares = new LinkedList<IMiddleware>();



        /// <summary>
        /// 获取或设置会话的心跳检测时间间隔
        /// TimeSpan.Zero为不检测
        /// </summary>
        public TimeSpan KeepAlivePeriod { get; set; }

        /// <summary>
        /// 获取是否已处在监听中
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// 获取所监听的本地IP和端口
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// 获取服务器证书
        /// </summary>
        public X509Certificate Certificate { get; private set; }

        /// <summary>
        /// 获取事件对象
        /// </summary>
        public Events Events { get; private set; }

        /// <summary>
        /// 获取会话提供者
        /// </summary>
        public ISessionManager SessionManager { get; private set; }

        /// <summary>
        /// Tcp监听服务
        /// </summary>
        public TcpListener()
        {
            this.middlewares.AddLast(new LastMiddlerware());
            this.Events = new Events();
            this.SessionManager = this.workSessions;
        }

        /// <summary>
        /// 使用SSL安全传输
        /// </summary>
        /// <param name="cer">证书</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void UseSSL(X509Certificate cer)
        {
            if (cer == null)
            {
                throw new ArgumentNullException();
            }
            if (this.IsListening == true)
            {
                throw new InvalidOperationException("实例已经IsListening，不能UseSSL");
            }
            this.Certificate = cer;
        }

        /// <summary>
        /// Tcp监听服务
        /// </summary>
        /// <param name="certificate">服务器证书</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TcpListener(X509Certificate certificate)
            : this()
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            this.Certificate = certificate;
        }

        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <typeparam name="TMiddleware">中间值类型</typeparam>
        /// <returns></returns>
        public TMiddleware Use<TMiddleware>() where TMiddleware : IMiddleware
        {
            var middleware = Activator.CreateInstance<TMiddleware>();
            return this.Use(middleware);
        }

        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <typeparam name="TMiddleware">中间值类型</typeparam>
        /// <param name="middleware">中间件实例</param>
        /// <returns></returns>
        public TMiddleware Use<TMiddleware>(TMiddleware middleware) where TMiddleware : IMiddleware
        {
            this.Use((IMiddleware)middleware);
            return middleware;
        }

        /// <summary>
        /// 使用中间件
        /// </summary>
        /// <param name="middleware">中间件</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Use(IMiddleware middleware)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException();
            }

            this.middlewares.AddBefore(this.middlewares.Last, middleware);
            var node = this.middlewares.First;
            while (node.Next != null)
            {
                node.Value.Next = node.Next.Value;
                node = node.Next;
            }
        }

        /// <summary>
        /// 开始启动监听
        /// 如果IsListening为true，将不产生任何作用
        /// </summary>
        /// <param name="port">本机tcp端口</param>
        /// <exception cref="SocketException"></exception>
        public void Start(int port)
        {
            var backlog = 1024;
            this.Start(port, backlog);
        }

        /// <summary>
        /// 开始启动监听
        /// 如果IsListening为true，将不产生任何作用
        /// </summary>
        /// <param name="port">本机tcp端口</param>
        /// <param name="backlog">挂起连接队列的最大长度</param>
        /// <exception cref="SocketException"></exception>
        public void Start(int port, int backlog)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, port);
            this.Start(localEndPoint, backlog);
        }

        /// <summary>
        /// 开始启动监听
        /// 如果IsListening为true，将不产生任何作用
        /// </summary>
        /// <param name="localEndPoint">本机ip和端口</param>
        /// <param name="backlog">挂起连接队列的最大长度</param>
        /// <exception cref="SocketException"></exception>
        public void Start(IPEndPoint localEndPoint, int backlog)
        {
            if (this.IsListening == true)
            {
                return;
            }
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(localEndPoint);
            this.listenSocket.Listen(backlog);

            this.acceptArg = new SocketAsyncEventArgs();
            this.acceptArg.Completed += (sender, e) => this.AcceptSocketCompleted(e);
            this.AcceptSocketAsync(this.acceptArg);

            this.LocalEndPoint = localEndPoint;
            this.IsListening = true;
        }

        /// <summary>
        /// 开始一次接受连接请求操作
        /// </summary>
        /// <param name="arg">接受参数</param>     
        private void AcceptSocketAsync(SocketAsyncEventArgs arg)
        {
            if (this.listenSocket != null)
            {
                arg.AcceptSocket = null;
                if (this.listenSocket.AcceptAsync(arg) == false)
                {
                    this.AcceptSocketCompleted(arg);
                }
            }
        }

        /// <summary>
        /// 连接请求IO完成
        /// </summary>
        /// <param name="arg">连接参数</param>
        private void AcceptSocketCompleted(SocketAsyncEventArgs arg)
        {
            var socket = arg.AcceptSocket;
            var error = arg.SocketError;
            this.OnAccept(socket, error);
            this.AcceptSocketAsync(arg);
        }

        /// <summary>
        /// 当接收到Socket连接时
        /// </summary>
        /// <param name="socket">socket</param>
        /// <param name="socketError">状态</param>
        private void OnAccept(Socket socket, SocketError socketError)
        {
            if (socketError == SocketError.Success)
            {
                this.BuildSession(socket);
            }
            else
            {
                var exception = new SocketException((int)socketError);
                this.Events.RaiseException(this, exception);
            }
        }

        /// <summary>
        /// 创建会话对象
        /// </summary>
        /// <returns></returns>
        private TcpSessionBase CreateSession()
        {
            var session = this.freeSessions.Take();
            if (session != null)
            {
                return session;
            }

            if (this.Certificate == null)
            {
                return new IocpTcpSession();
            }
            else
            {
                return new SslTcpSession(this.Certificate);
            }
        }

        /// <summary>
        /// 创建上下文对象
        /// </summary>
        /// <param name="session">当前会话</param>
        /// <returns></returns>
        private IContenxt CreateContext(TcpSessionBase session)
        {
            return new Context
            {
                Session = session,
                StreamReader = session.StreamReader,
                AllSessions = this.SessionManager
            };
        }

        /// <summary>
        /// 生成一个会话对象
        /// </summary>
        /// <param name="socket">要绑定的socket</param>
        private void BuildSession(Socket socket)
        {
            // 创建会话，绑定处理委托
            var session = this.CreateSession();
            session.ReceiveAsyncHandler = this.InvokeSessionAsync;
            session.DisconnectHandler = this.RecyceSession;
            session.CloseHandler = this.RecyceSession;

            session.Bind(socket);
            session.TrySetKeepAlive(this.KeepAlivePeriod);
            this.workSessions.Add(session);

            // 通知已连接
            var context = this.CreateContext(session);
            this.Events.RaiseConnected(this, context);

            // 开始接收数据
            session.LoopReceive();
        }

        /// <summary>
        /// 执行会话请求处理
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <returns></returns>
        private async Task InvokeSessionAsync(TcpSessionBase session)
        {
            try
            {
                var context = this.CreateContext(session);
                await this.middlewares.First.Value.Invoke(context);
            }
            catch (Exception ex)
            {
                this.Events.RaiseException(this, ex);
            }
        }

        /// <summary>
        /// 回收复用会话对象
        /// 关闭会话并通知连接断开
        /// </summary>
        /// <param name="session">会话对象</param>
        private void RecyceSession(TcpSessionBase session)
        {
            if (this.workSessions.Remove(session) == true)
            {
                var context = this.CreateContext(session);
                this.Events.RaiseDisconnected(this, context);
                session.Close(false);
                this.freeSessions.Add(session);
            }
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
        ~TcpListener()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.IsListening == true)
            {
                this.listenSocket.Dispose();
            }

            this.acceptArg.Dispose();
            this.workSessions.Dispose();
            this.freeSessions.Dispose();
            this.middlewares.Clear();

            if (disposing == true)
            {
                this.listenSocket = null;
                this.acceptArg = null;
                this.middlewares = null;
                this.workSessions = null;
                this.freeSessions = null;

                this.LocalEndPoint = null;
                this.IsListening = false;
                this.KeepAlivePeriod = TimeSpan.Zero;
                this.Events = null;
            }
        }
        #endregion

    }
}
