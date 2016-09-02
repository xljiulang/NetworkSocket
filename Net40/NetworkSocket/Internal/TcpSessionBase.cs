using NetworkSocket.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkSocket
{
    /// <summary>
    /// Tcp会话抽象类
    /// </summary>
    internal abstract class TcpSessionBase : ISession, IDisposable
    {
        /// <summary>
        /// 是否已关闭
        /// </summary>
        private bool socketClosed = true;

        /// <summary>
        /// socket同步锁
        /// </summary>
        private object socketRoot = new object();


        /// <summary>
        /// 获取绑定的Socket对象
        /// </summary>        
        protected Socket Socket { get; private set; }

        /// <summary>
        /// 获取或设置等待发送的数量
        /// </summary>
        protected int PendingSendCount = 0;

        /// <summary>
        /// 获取待发送的ByeRange集合
        /// </summary>
        protected ConcurrentQueue<IByteRange> PendingSendByteRanges { get; private set; }


        /// <summary>
        /// 处理和分析收到的数据的委托
        /// </summary>
        public Action<TcpSessionBase> ReceiveHandler;

        /// <summary>
        /// 连接断开委托   
        /// </summary>
        public Action<TcpSessionBase> DisconnectHandler;

        /// <summary>
        /// 关闭时的委托
        /// </summary>
        public Action<TcpSessionBase> CloseHandler;


        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.socketClosed == false && this.Socket != null && this.Socket.Connected;
            }
        }

        /// <summary>
        /// 获取会话是否提供SSL/TLS安全
        /// </summary>
        public abstract bool IsSecurity { get; }

        /// <summary>
        /// 获取用户附加数据
        /// </summary>
        public ITag Tag { get; private set; }

        /// <summary>
        /// 获取会话的协议
        /// </summary>
        public string Protocol { get; private set; }

        /// <summary>
        /// 获取会话的包装对象
        /// </summary>
        public IWrapper Wrapper { get; private set; }

        /// <summary>
        /// 获取接收到的未处理数据
        /// </summary>      
        public NsStream RecvStream { get; private set; }

        /// <summary>
        /// 获取本机终结点
        /// </summary>
        public EndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public EndPoint RemoteEndPoint { get; private set; }


        /// <summary>
        /// 表示会话对象
        /// </summary>  
        public TcpSessionBase()
        {
            this.Tag = new Tag();
            this.RecvStream = new NsStream();
            this.PendingSendByteRanges = new ConcurrentQueue<IByteRange>();
        }

        /// <summary>
        /// 绑定一个Socket对象
        /// </summary>
        /// <param name="socket">套接字</param>
        public virtual void Bind(Socket socket)
        {
            this.Socket = socket;
            this.socketClosed = false;
            this.PendingSendCount = 0;

            if (this.PendingSendByteRanges.Count > 0)
            {
                this.PendingSendByteRanges = new ConcurrentQueue<IByteRange>();
            }

            this.RecvStream.Clear();
            this.Tag.Clear();
            this.SetProtocolWrapper(null, null);
            this.LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
            this.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
        }

        /// <summary>
        /// 开始循环接收数据 
        /// </summary>
        public abstract void LoopReceive();

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>         
        public abstract void SendAsync(IByteRange byteRange);

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>
        public abstract void Send(IByteRange byteRange);

        /// <summary>
        /// 主动断开和远程端的连接  
        /// </summary>
        /// <param name="waitForSendComplete">是否等待数据发送完成</param>
        /// <returns></returns>
        public bool Close(bool waitForSendComplete)
        {
            lock (this.socketRoot)
            {
                if (this.socketClosed == true)
                {
                    return false;
                }

                if (waitForSendComplete == false)
                {
                    this.TryInvokeAction(() => this.Socket.Shutdown(SocketShutdown.Both));
                }
                else
                {
                    this.TryInvokeAction(() => this.Socket.Shutdown(SocketShutdown.Receive));
                    var spinWait = new SpinWait();
                    while (this.IsConnected && this.PendingSendCount > 0)
                    {
                        spinWait.SpinOnce();
                    }
                    this.TryInvokeAction(() => this.Socket.Shutdown(SocketShutdown.Send));
                }

                this.Socket.Dispose();
                this.socketClosed = true;
                return true;
            }
        }

        /// <summary>     
        /// 等待缓冲区数据发送完成
        /// 然后断开和远程端的连接   
        /// </summary>     
        void ISession.Close()
        {
            if (this.Close(true) == false)
            {
                return;
            }

            if (this.CloseHandler != null)
            {
                this.CloseHandler(this);
            }
        }

        /// <summary>
        /// 获取会话的协议是否和protocol匹配
        /// </summary>
        /// <param name="protocol">协议名</param>
        /// <returns></returns>
        public bool? IsProtocol(string protocol)
        {
            if (string.IsNullOrEmpty(this.Protocol) == true)
            {
                return null;
            }
            return string.Equals(this.Protocol, protocol, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 设置协议和会话包装对象
        /// </summary>
        /// <param name="protocol">协议</param>
        /// <param name="wrapper">会话的包装对象</param>
        public void SetProtocolWrapper(string protocol, IWrapper wrapper)
        {
            this.Protocol = protocol;
            this.Wrapper = wrapper;
        }

        /// <summary>
        /// 设置会话的心跳包
        /// </summary>
        /// <param name="keepAlivePeriod">时间间隔</param>
        /// <returns></returns>
        public bool TrySetKeepAlive(TimeSpan keepAlivePeriod)
        {
            if (keepAlivePeriod == TimeSpan.Zero)
            {
                return false;
            }

            if (this.Socket == null)
            {
                return false;
            }

            var period = (int)keepAlivePeriod.TotalMilliseconds;
            return this.TrySetKeepAlive(Socket, period, period);
        }

        /// <summary>
        /// 设置会话的心跳包
        /// </summary>
        /// <param name="socket">客户端</param>
        /// <param name="dueTime">延迟的时间量（以毫秒为单位）</param>
        /// <param name="period">时间间隔（以毫秒为单位）</param>
        /// <returns></returns>
        private bool TrySetKeepAlive(Socket socket, int dueTime, int period)
        {
            var inOptionValue = new byte[12];
            var outOptionValue = new byte[12];

            ByteConverter.ToBytes(1, Endians.Little).CopyTo(inOptionValue, 0);
            ByteConverter.ToBytes(dueTime, Endians.Little).CopyTo(inOptionValue, 4);
            ByteConverter.ToBytes(period, Endians.Little).CopyTo(inOptionValue, 8);

            try
            {
                socket.IOControl(IOControlCode.KeepAliveValues, inOptionValue, outOptionValue);
                return true;
            }
            catch (NotSupportedException)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, inOptionValue);
                return true;
            }
            catch (NotImplementedException)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, inOptionValue);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试执行方法
        /// </summary>
        /// <param name="action">方法</param>
        protected bool TryInvokeAction(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试执行方法
        /// </summary>
        /// <param name="action">方法</param>
        /// <param name="ex">异常处理</param>
        protected bool TryInvokeAction(Action action, Action ex)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception)
            {
                ex.Invoke();
                return false;
            }
        }

        /// <summary>
        /// 尝试执行方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">方法</param>
        /// <returns></returns>
        protected T TryInvokeFunc<T>(Func<T> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.RemoteEndPoint == null ? string.Empty : this.RemoteEndPoint.ToString();
        }


        #region IDisposable

        /// <summary>
        /// 获取是否已释放
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
        ~TcpSessionBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            this.Close(false);
            this.RecvStream.Dispose();

            if (disposing)
            {
                this.socketRoot = null;
                this.Socket = null;
                this.PendingSendByteRanges = null;
                this.Tag = null;
                this.RecvStream = null;
                this.CloseHandler = null;
                this.DisconnectHandler = null;
                this.ReceiveHandler = null;
            }
        }
        #endregion
    }
}
