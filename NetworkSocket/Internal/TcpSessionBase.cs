using NetworkSocket.Streams;
using NetworkSocket.Util;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// 处理和分析收到的数据的委托
        /// </summary>
        public Func<TcpSessionBase, Task> ReceiveAsyncHandler;

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
        public Protocol Protocol { get; private set; }

        /// <summary>
        /// 获取会话的包装对象
        /// </summary>
        public IWrapper Wrapper { get; private set; }

        /// <summary>
        /// 获取接收到数据读取器
        /// </summary>      
        public SessionStreamReader StreamReader { get; private set; }

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
            this.StreamReader = new SessionStreamReader(new SessionStream());
        }

        /// <summary>
        /// 绑定一个Socket对象
        /// </summary>
        /// <param name="socket">套接字</param>
        public virtual void Bind(Socket socket)
        {
            this.Socket = socket;
            this.socketClosed = false;

            this.StreamReader.Clear();
            this.Tag.ID = null;
            ((IDictionary)this.Tag).Clear();
            this.UnSubscribe();
            this.SetProtocolWrapper(Protocol.None, null);
            this.LocalEndPoint = socket.LocalEndPoint;
            this.RemoteEndPoint = socket.RemoteEndPoint;
        }

        /// <summary>
        /// 开始循环接收数据 
        /// </summary>
        public abstract void LoopReceive();

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public virtual int Send(ArraySegment<byte> byteRange)
        {
            if (byteRange.Array == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            return this.Socket.Send(byteRange.Array, byteRange.Offset, byteRange.Count, SocketFlags.None);
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public virtual int Send(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }
            return this.Socket.Send(buffer);
        }

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
                    try
                    {
                        this.Socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception)
                    {
                    }
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
        /// 设置协议和会话包装对象
        /// </summary>
        /// <param name="protocol">协议</param>
        /// <param name="wrapper">会话的包装对象</param>
        public void SetProtocolWrapper(Protocol protocol, IWrapper wrapper)
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

            ByteConverter.ToBytes(1, ByteConverter.Endian).CopyTo(inOptionValue, 0);
            ByteConverter.ToBytes(dueTime, ByteConverter.Endian).CopyTo(inOptionValue, 4);
            ByteConverter.ToBytes(period, ByteConverter.Endian).CopyTo(inOptionValue, 8);

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
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.RemoteEndPoint == null ? string.Empty : this.RemoteEndPoint.ToString();
        }

        #region ISubscriber

        /// <summary>
        /// 订阅表
        /// </summary>
        private readonly Dictionary<string, Action<object>> subscribeTable = new Dictionary<string, Action<object>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 订阅
        /// 多次订阅同个频道会覆盖前者的handler
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="handler">处理者</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Subscribe(string channel, Action<object> handler)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentNullException("channel");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            this.subscribeTable[channel] = handler;
        }

        /// <summary>
        /// 取消所有订阅
        /// </summary>
        public void UnSubscribe()
        {
            this.subscribeTable.Clear();
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="channel">频道</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool UnSubscribe(string channel)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentNullException();
            }
            return this.subscribeTable.Remove(channel);
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="data">数据</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool Publish(string channel, object data)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentNullException("channel");
            }

            Action<object> action;
            if (this.subscribeTable.TryGetValue(channel, out action))
            {
                action(data);
                return true;
            }
            return false;
        }
        #endregion


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
            this.StreamReader.Stream.Dispose();

            if (disposing)
            {
                this.socketRoot = null;
                this.Socket = null;
                this.Tag = null;
                this.StreamReader = null;
                this.CloseHandler = null;
                this.DisconnectHandler = null;
                this.ReceiveAsyncHandler = null;
            }
        }
        #endregion
    }
}
