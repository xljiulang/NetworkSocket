using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Net.Security;
using System.Security.Authentication;

namespace NetworkSocket
{
    /// <summary>
    /// 表示Tcp客户端抽象类
    /// </summary>   
    public abstract class TcpClientBase : IWrapper, IDisposable
    {
        /// <summary>
        /// 会话对象
        /// </summary>
        private TcpSessionBase session;

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return this.session.RemoteEndPoint;
            }
        }

        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.session.IsConnected;
            }
        }

        /// <summary>
        /// 获取用户附加数据
        /// </summary>
        public ITag Tag
        {
            get
            {
                return this.session.Tag;
            }
        }

        /// <summary>
        /// 获取或设置断线自动重连的时间间隔 
        /// 设置为TimeSpan.Zero表示不自动重连
        /// </summary>
        public TimeSpan AutoReconnect { get; set; }


        /// <summary>
        /// Tcp客户端抽象类
        /// </summary>
        public TcpClientBase()
        {
            this.session = new IocpTcpSession();
            this.BindHandler(this.session);
        }

        /// <summary>
        /// SSL支持的Tcp客户端抽象类
        /// </summary>
        /// <param name="targetHost">目标主机</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TcpClientBase(string targetHost)
            : this(targetHost, null)
        {
        }

        /// <summary>
        /// SSL支持的Tcp客户端抽象类
        /// </summary>  
        /// <param name="targetHost">目标主机</param>
        /// <param name="certificateValidationCallback">远程证书验证回调</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TcpClientBase(string targetHost, RemoteCertificateValidationCallback certificateValidationCallback)
        {
            this.session = new SslTcpSession(targetHost, certificateValidationCallback);
            this.BindHandler(this.session);
        }

        /// <summary>
        /// 绑定会话的处理方法
        /// </summary>
        /// <param name="session">会话</param>
        private void BindHandler(TcpSessionBase session)
        {
            session.ReceiveHandler = this.ReceiveHandler;
            session.DisconnectHandler = this.DisconnectHandler;
        }

        /// <summary>
        /// 连接到远程端
        /// </summary>
        /// <param name="hostNameOrAddress">域名或ip地址</param>
        /// <param name="port">远程端口</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public Task<bool> Connect(string hostNameOrAddress, int port)
        {
            var ipAddress = Dns.GetHostAddresses(hostNameOrAddress);
            return this.Connect(ipAddress.Last(), port);
        }

        /// <summary>
        /// 连接到远程终端       
        /// </summary>
        /// <param name="ip">远程ip</param>
        /// <param name="port">远程端口</param>
        /// <returns></returns>
        public Task<bool> Connect(IPAddress ip, int port)
        {
            return this.Connect(new IPEndPoint(ip, port));
        }

        /// <summary>
        /// 连接到远程终端 
        /// </summary>
        /// <param name="remoteEndPoint">远程ip和端口</param> 
        /// <exception cref="AuthenticationException"></exception>
        /// <returns></returns>
        public Task<bool> Connect(IPEndPoint remoteEndPoint)
        {
            var taskSource = new TaskCompletionSource<bool>();
            if (this.IsConnected)
            {
                taskSource.TrySetResult(true);
                return taskSource.Task;
            }

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var connectArg = new SocketAsyncEventArgs { RemoteEndPoint = remoteEndPoint, UserToken = taskSource };
            connectArg.Completed += this.ConnectCompleted;

            if (socket.ConnectAsync(connectArg) == false)
            {
                this.ConnectCompleted(socket, connectArg);
            }
            return taskSource.Task;
        }


        /// <summary>
        /// 连接完成事件
        /// </summary>
        /// <param name="sender">连接者</param>
        /// <param name="e">事件参数</param>
        private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            var socket = sender as Socket;
            var taskSource = e.UserToken as TaskCompletionSource<bool>;
            var result = e.SocketError == SocketError.Success;

            if (result == true)
            {
                this.session.Bind(socket);
                this.session.LoopReceive();
            }
            else
            {
                socket.Dispose();
            }

            e.Completed -= this.ConnectCompleted;
            e.Dispose();
            taskSource.TrySetResult(result);
        }


        /// <summary>
        /// 接收处理
        /// </summary>
        /// <param name="session">会话</param>
        private void ReceiveHandler(TcpSessionBase session)
        {
            this.OnReceive(session.RecvBuffer);
        }

        /// <summary>
        /// 关闭连接处理
        /// </summary>
        /// <param name="session">会话</param>
        private void DisconnectHandler(TcpSessionBase session)
        {
            session.Close(false);
            this.OnDisconnected();
            this.ReconnectLoop();
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法   
        /// </summary>       
        /// <param name="buffer">接收到的历史数据</param>
        /// <returns></returns>
        protected abstract void OnReceive(IReceiveBuffer buffer);


        /// <summary>
        /// 当与服务器断开连接后，将触发此方法
        /// </summary>       
        protected virtual void OnDisconnected()
        {
        }

        /// <summary>
        /// 循环尝试间隔地重连
        /// </summary>
        private void ReconnectLoop()
        {
            if (this.AutoReconnect == TimeSpan.Zero)
            {
                return;
            }

            Action<bool> action = (connected) =>
            {
                if (connected == false)
                {
                    Thread.Sleep((int)this.AutoReconnect.TotalMilliseconds);
                    this.ReconnectLoop();
                }
            };

            this.Connect(this.RemoteEndPoint).ContinueWith((t) => action(t.Result));
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>  
        /// <exception cref="ArgumentNullException"></exception>        
        /// <exception cref="SocketException"></exception>
        public void Send(IByteRange byteRange)
        {
            this.session.Send(byteRange);
        }

        /// <summary>     
        /// 等待缓冲区数据发送完成
        /// 然后断开和远程端的连接   
        /// </summary>     
        public void Close()
        {
            this.session.Close(true);
        }

        /// <summary>
        /// 设置会话的心跳包
        /// </summary>
        /// <param name="keepAlivePeriod">时间间隔</param>
        /// <returns></returns>
        public bool TrySetKeepAlive(TimeSpan keepAlivePeriod)
        {
            return this.session.TrySetKeepAlive(keepAlivePeriod);
        }

        /// <summary>
        /// 还原到包装前
        /// </summary>
        /// <returns></returns>
        public ISession UnWrap()
        {
            return this.session;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            this.session.Dispose();
        }
    }
}