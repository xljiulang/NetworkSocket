using NetworkSocket.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 表示连接后的委托
    /// </summary>
    /// <param name="sender">服务</param>
    /// <param name="context">上下文</param>
    public delegate void ConnectedHandler(object sender, IContenxt context);

    /// <summary>
    /// 表示断开连接后的委托
    /// </summary>
    /// <param name="sender">服务</param>
    /// <param name="context">上下文</param>
    public delegate void DisconnectedHandler(object sender, IContenxt context);

    /// <summary>
    /// 表示异常时的委托
    /// </summary>
    /// <param name="sender">服务</param>
    /// <param name="exception">异常</param>
    public delegate void ExceptionHandler(object sender, Exception exception);


    /// <summary>
    /// 表示Tcp服务器
    /// </summary>
    public class TcpServer : IServer
    {
        /// <summary>
        /// 所有中间件
        /// </summary>
        private LinkedList<IMiddleware> middlewares = new LinkedList<IMiddleware>();

        /// <summary>
        /// 已回收的会话对象
        /// </summary>        
        private TcpSessionQueue freeSessions = new TcpSessionQueue();

        /// <summary>
        /// 所有工作中的会话对象
        /// </summary>
        private TcpSessionCollection workSessions = new TcpSessionCollection();

        /// <summary>
        /// 会话连接后事件
        /// </summary>
        public event ConnectedHandler OnConnected;

        /// <summary>
        /// 会话断开后事件
        /// </summary>
        public event DisconnectedHandler OnDisconnected;

        /// <summary>
        /// 服务异常事件
        /// </summary>
        public event ExceptionHandler OnException;

        /// <summary>
        /// 获取或设置会话的心跳检测时间间隔
        /// TimeSpan.Zero为不检测
        /// </summary>
        public TimeSpan KeepAlivePeriod { get; set; }

        /// <summary>
        /// Tcp服务
        /// </summary>
        public TcpServer()
        {
            this.middlewares.AddLast(new LastMiddlerware());
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
        /// 当接收到Socket连接时
        /// </summary>
        /// <param name="socket">socket</param>
        /// <param name="socketError">状态</param>
        void IServer.OnAccept(Socket socket, SocketError socketError)
        {
            if (socketError != SocketError.Success)
            {
                if (this.OnException != null)
                {
                    var exception = new SocketException((int)socketError);
                    this.OnException(this, exception);
                }
            }
            else
            {
                this.BuildSession(socket);
            }
        }

        /// <summary>
        /// 生成一个会话对象
        /// </summary>
        /// <param name="socket">要绑定的socket</param>
        private void BuildSession(Socket socket)
        {
            var session = this.freeSessions.Take();
            if (session == null)
            {
                session = new TcpSession();
            }

            // 绑定处理委托
            session.ReceiveHandler = () => this.OnSessionRequest(session);
            session.DisconnectHandler = () => this.RecyceSession(session);
            session.CloseHandler = () => this.RecyceSession(session);

            session.Bind(socket);
            session.TrySetKeepAlive(this.KeepAlivePeriod);
            this.workSessions.Add(session);

            // 通知已连接
            if (this.OnConnected != null)
            {
                var context = new Context(session, session.RecvStream, this.workSessions);
                this.OnConnected(this, context);
            }
            // 开始接收数据
            session.TryReceiveAsync();
        }

        /// <summary>
        /// 回收复用会话对象
        /// 关闭会话并通知连接断开
        /// </summary>
        /// <param name="session">会话对象</param>
        private void RecyceSession(TcpSession session)
        {
            if (this.workSessions.Remove(session) == true)
            {
                if (this.OnDisconnected != null)
                {
                    var context = new Context(session, session.RecvStream, this.workSessions);
                    this.OnDisconnected(this, context);
                }

                session.Close(false);
                this.freeSessions.Add(session);
            }
        }


        /// <summary>
        /// 收到会话对象的请求            
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <returns></returns>
        private void OnSessionRequest(TcpSession session)
        {
            try
            {
                var context = new Context(session, session.RecvStream, this.workSessions);
                var task = this.middlewares.First.Value.Invoke(context);
                if (task.Status == TaskStatus.Created) task.Start();
            }
            catch (Exception ex)
            {
                if (this.OnException != null)
                {
                    this.OnException(this, ex);
                }
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
        ~TcpServer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            this.workSessions.Dispose();
            this.freeSessions.Dispose();
            this.middlewares.Clear();

            if (disposing == true)
            {
                this.middlewares = null;
                this.workSessions = null;
                this.freeSessions = null;
            }
        }
        #endregion

    }
}
