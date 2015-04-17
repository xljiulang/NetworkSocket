
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;


namespace NetworkSocket
{
    /// <summary>
    /// Tcp服务端抽象类
    /// 提供对客户端池的初始化、自动回收重用、在线客户端列表维护功能
    /// 提供客户端连接、断开通知功能
    /// 所有Tcp服务端都派生于此类
    /// </summary>
    /// <typeparam name="T">发送数据包协议</typeparam>
    /// <typeparam name="TRecv">接收到的数据包类型</typeparam>
    [DebuggerDisplay("IsListening = {IsListening}")]
    public abstract class TcpServerBase<T, TRecv> : ITcpServer<T> where T : PacketBase
    {
        /// <summary>
        /// 服务socket
        /// </summary>
        private volatile Socket listenSocket;

        /// <summary>
        /// 请求参数
        /// </summary>
        private SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();

        /// <summary>
        /// 空闲客户端池
        /// </summary>
        private SocketClientBag<T, TRecv> clientBag = new SocketClientBag<T, TRecv>();


        /// <summary>
        /// 获取所有连接的客户端对象   
        /// </summary>
        public ClientCollection<T> AliveClients { get; private set; }

        /// <summary>
        /// 获取所监听的本地IP和端口
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// 获取服务是否已处在监听中
        /// </summary>
        public bool IsListening { get; private set; }


        /// <summary>
        /// Tcp服务端抽象类
        /// </summary> 
        public TcpServerBase()
        {
            this.AliveClients = new ClientCollection<T>();
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
            this.acceptArg.Completed += (sender, e) => { this.AcceptArgCompleted(e); };
            this.AcceptClient(this.acceptArg);

            this.LocalEndPoint = localEndPoint;
            this.IsListening = true;
        }

        /// <summary>
        /// 开始一次接受连接请求操作
        /// </summary>
        /// <param name="arg">连接参数</param>     
        private void AcceptClient(SocketAsyncEventArgs arg)
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
            if (arg.SocketError == SocketError.Success)
            {
                // 从池中取出SocketAsync
                var client = this.clientBag.Take();

                // 绑定处理委托               
                client.SendHandler = (packet) => this.OnSend(client, packet);
                client.ReceiveHandler = (builder) => this.OnReceive(client, builder);
                client.RecvCompleteHandler = (packet) => this.OnRecvCompleteHandleWithTask(client, packet);
                client.DisconnectHandler = () => this.RecyceClient(client);

                // SocketAsync与socket绑定    
                client.Bind(arg.AcceptSocket);
                // 添加到活动列表
                this.AliveClients.Add(client);
                // 通知已连接
                this.OnConnect(client);
                // 开始接收数据
                client.BeginReceive();
            }

            // 处理后继续接收
            this.AcceptClient(arg);
        }

        /// <summary>
        /// 发送数据包后触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        protected virtual void OnSend(IClient<T> client, T packet)
        {
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法       
        /// 返回的每个数据包将触发一次OnRecvComplete方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="builder">接收到的历史数据</param>
        /// <returns></returns>
        protected abstract IEnumerable<TRecv> OnReceive(IClient<T> client, ByteBuilder builder);

        /// <summary>
        /// 使用Task来处理OnRecvComplete业务方法
        /// 重写此方法，使用LimitedTask来代替系统默认的Task可以控制并发数
        /// 例：myLimitedTask.Run(() => this.OnRecvComplete(client, tRecv));
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="tRecv">接收到的数据类型</param>
        protected virtual void OnRecvCompleteHandleWithTask(IClient<T> client, TRecv tRecv)
        {
            Task.Factory.StartNew(() => this.OnRecvComplete(client, tRecv));
        }

        /// <summary>
        /// 当收到到数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="tRecv">接收到的数据类型</param>
        protected virtual void OnRecvComplete(IClient<T> client, TRecv tRecv)
        {
        }


        /// <summary>
        /// 回收复用客户端对象
        /// 关闭客户端并通知连接断开
        /// </summary>
        /// <param name="client">客户端对象</param>
        private void RecyceClient(SocketClient<T, TRecv> client)
        {
            var recyced = this.AliveClients.Remove(client) == false;
            if (recyced == true)
            {
                return;
            }

            this.OnDisconnect(client);
            client.Close();
            client.TagData.Clear();
            this.clientBag.Add(client);
        }

        /// <summary>
        /// 当客户端断开连接时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>     
        protected virtual void OnDisconnect(IClient<T> client)
        {
        }

        /// <summary>
        /// 当客户端连接时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        protected virtual void OnConnect(IClient<T> client)
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
            try
            {
                if (this.listenSocket != null) this.listenSocket.Dispose();
            }
            finally
            {
                this.listenSocket = null;
            }

            foreach (IDisposable client in this.AliveClients)
            {
                client.Dispose();
            }

            this.clientBag.Dispose();
            this.acceptArg.Dispose();

            if (disposing)
            {
                this.AliveClients.Clear();
                this.AliveClients = null;

                this.acceptArg = null;
                this.clientBag = null;
                this.LocalEndPoint = null;
                this.IsListening = false;
            }
        }
        #endregion
    }
}
