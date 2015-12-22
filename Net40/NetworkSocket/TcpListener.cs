
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using NetworkSocket.Exceptions;


namespace NetworkSocket
{
    /// <summary>
    /// 表示Tcp监听器   
    /// </summary>  
    [DebuggerDisplay("IsListening = {IsListening}")]
    public class TcpListener : IListener
    {
        /// <summary>
        /// 服务实例
        /// </summary>
        private IServer server;

        /// <summary>
        /// 用于监听的socket
        /// </summary>
        private volatile Socket listenSocket;

        /// <summary>
        /// 接受参数
        /// </summary>
        private SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();


        /// <summary>
        /// 获取是否已处在监听中
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// 获取所监听的本地IP和端口
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }


        /// <summary>
        /// Tcp监听器  
        /// </summary> 
        public TcpListener()
        {
            this.server = new TcpServer();
        }

        /// <summary>
        /// 开始启动监听
        /// 如果IsListening为true，将不产生任何作用
        /// </summary>
        /// <param name="server">服务实例</param>
        /// <param name="port">本机tcp端口</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SocketException"></exception>
        public void Start(IServer server, int port)
        {
            if (this.IsListening)
            {
                return;
            }

            if (server == null)
            {
                throw new ArgumentNullException("server");
            }

            this.server = server;
            var localEndPoint = new IPEndPoint(IPAddress.Any, port);
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(localEndPoint);
            this.listenSocket.Listen(100);

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
            this.server.OnAccept(socket, error);
            this.AcceptSocketAsync(arg);
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
            if (this.listenSocket != null)
            {
                this.listenSocket.Dispose();
            }
            this.acceptArg.Dispose();
            this.server.Dispose();

            if (disposing)
            {
                this.listenSocket = null;
                this.acceptArg = null;
                this.LocalEndPoint = null;
                this.IsListening = false;
            }
        }
        #endregion
    }
}
