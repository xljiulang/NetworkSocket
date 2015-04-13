
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
    /// <typeparam name="T">PacketBase派生类型</typeparam>
    [DebuggerDisplay("IsListening = {IsListening}")]
    public abstract class TcpServerBase<T> : ITcpServer<T> where T : PacketBase
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
        /// 客户端连接池
        /// </summary>
        private SocketAsyncBag<T> clientBag = new SocketAsyncBag<T>();


        /// <summary>
        /// 获取所有连接的客户端对象   
        /// </summary>
        public SocketAsyncCollection<T> AliveClients { get; private set; }

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
            this.AliveClients = new SocketAsyncCollection<T>();
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
                client.DisconnectHandler = () => this.OnClientDisconnect(client);

                // SocketAsync与socket绑定    
                client.BindSocket(arg.AcceptSocket);
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
        /// 当接收到远程端的数据时，将触发此方法
        /// 此方法用于处理和分析收到的数据
        /// 如果得到一个数据包，将触发OnRecvComplete方法
        /// [注]这里只需处理一个数据包的流程
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="recvBuilder">接收到的历史数据</param>
        /// <returns>如果不够一个数据包，则请返回null</returns>
        protected abstract T OnReceive(SocketAsync<T> client, ByteBuilder recvBuilder);


        /// <summary>
        /// 发送之前触发
        /// </summary>      
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        protected virtual void OnSend(SocketAsync<T> client, T packet)
        {
        }

        /// <summary>
        /// 使用Task来处理OnRecvComplete业务方法
        /// 重写此方法，使用LimitedTask来代替系统默认的Task可以控制并发数
        /// 例：myLimitedTask.Run(() => this.OnRecvComplete(client, packet));
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">封包</param>
        protected virtual void OnRecvCompleteHandleWithTask(SocketAsync<T> client, T packet)
        {
            Task.Factory.StartNew(() => this.OnRecvComplete(client, packet));
        }

        /// <summary>
        /// 当收到到数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        protected virtual void OnRecvComplete(SocketAsync<T> client, T packet)
        {
        }

        /// <summary>
        /// 客户端socket关闭
        /// </summary>
        /// <param name="client">客户端</param>     
        private void OnClientDisconnect(SocketAsync<T> client)
        {
            this.CloseClient(client);
        }

        /// <summary>
        /// 当客户端断开连接时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>     
        protected virtual void OnDisconnect(SocketAsync<T> client)
        {
        }

        /// <summary>
        /// 当客户端连接时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        protected virtual void OnConnect(SocketAsync<T> client)
        {
        }


        /// <summary>
        /// 关闭并复用客户端对象
        /// </summary>
        /// <param name="client">客户端对象</param>
        public bool CloseClient(SocketAsync<T> client)
        {
            if (client.IsBounded == false)
            {
                return false;
            }

            this.AliveClients.Remove(client);
            this.OnDisconnect(client);
            client.UnBindSocket();
            this.clientBag.Add(client);
            return true;
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

            foreach (var client in this.AliveClients)
            {
                ((IDisposable)client).Dispose();
            }

            while (this.clientBag.Count > 0)
            {
                ((IDisposable)this.clientBag.Take()).Dispose();
            }

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
