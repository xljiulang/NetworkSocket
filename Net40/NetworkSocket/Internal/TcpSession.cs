using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using NetworkSocket.Util;

namespace NetworkSocket
{
    /// <summary>
    /// 表示Tcp会话对象  
    /// </summary>        
    [DebuggerDisplay("{RemoteEndPoint}")]
    internal class TcpSession : ISession
    {
        /// <summary>
        /// socket
        /// </summary>        
        private Socket socket;

        /// <summary>
        /// socket同步锁
        /// </summary>
        private object socketRoot = new object();

        /// <summary>
        /// 是否已关闭
        /// </summary>
        private bool socketClosed = true;

        /// <summary>
        /// 等待发送的数量
        /// </summary>
        private int pendingSendCount = 0;

        /// <summary>
        /// 用于发送的SocketAsyncEventArgs
        /// </summary>
        private SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();

        /// <summary>
        /// 待发送的ByeRange集合
        /// </summary>
        private ConcurrentQueue<IByteRange> byteRangeQueue = new ConcurrentQueue<IByteRange>();

        /// <summary>
        /// 用于接收的SocketAsyncEventArgs
        /// </summary>
        private SocketAsyncEventArgs recvArg = new SocketAsyncEventArgs();


        /// <summary>
        /// 处理和分析收到的数据的委托
        /// </summary>
        public Action ReceiveHandler;

        /// <summary>
        /// 连接断开委托   
        /// </summary>
        public Action DisconnectHandler;

        /// <summary>
        /// 关闭时的委托
        /// </summary>
        public Action CloseHandler;


        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.socket != null && this.socket.Connected;
            }
        }

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
        public ReceiveStream RecvStream { get; private set; }

        /// <summary>
        /// 获取本机终结点
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }


        /// <summary>
        /// 表示会话对象
        /// </summary>  
        /// <exception cref="OutOfMemoryException"></exception>
        public TcpSession()
        {
            this.sendArg.Completed += this.SendCompleted;
            this.recvArg.Completed += this.RecvCompleted;

            this.Tag = new Tag();
            this.RecvStream = new ReceiveStream();

            BufferSetter.SetBuffer(this.sendArg);
            BufferSetter.SetBuffer(this.recvArg);
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
        /// 绑定一个Socket对象
        /// </summary>
        /// <param name="socket">套接字</param>
        public void Bind(Socket socket)
        {
            this.socket = socket;
            this.socketClosed = false;

            this.recvArg.SocketError = SocketError.Success;
            this.RecvStream.Clear();

            this.pendingSendCount = 0;
            this.sendArg.SocketError = SocketError.Success;

            if (this.byteRangeQueue.Count > 0)
            {
                this.byteRangeQueue = new ConcurrentQueue<IByteRange>();
            }

            this.Tag.Clear();
            this.SetProtocolWrapper(null, null);
            this.LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
            this.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
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

            if (this.socket == null)
            {
                return false;
            }

            var period = (int)keepAlivePeriod.TotalMilliseconds;
            return this.TrySetKeepAlive(socket, period, period);
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
        /// <returns></returns>
        private bool TryInvoke(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试开始接收数据
        /// </summary>
        public bool TryReceiveAsync()
        {
            if (this.socketClosed || this.IsConnected == false)
            {
                return false;
            }

            return this.TryInvoke(() =>
            {
                if (this.socket.ReceiveAsync(this.recvArg) == false)
                {
                    this.RecvCompleted(this.socket, this.recvArg);
                }
            });
        }

        /// <summary>
        /// 接收到数据事件
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="arg">参数</param>
        private void RecvCompleted(object sender, SocketAsyncEventArgs arg)
        {
            if (arg.BytesTransferred == 0 || arg.SocketError != SocketError.Success)
            {
                this.DisconnectHandler();
                return;
            }

            lock (this.RecvStream.SyncRoot)
            {
                this.RecvStream.Seek(0, SeekOrigin.End);
                this.RecvStream.Write(arg.Buffer, arg.Offset, arg.BytesTransferred);
                this.RecvStream.Seek(0, SeekOrigin.Begin);
                this.ReceiveHandler();
            }

            // 重新进行一次接收
            this.TryReceiveAsync();
        }


        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>  
        /// <exception cref="ArgumentNullException"></exception>        
        /// <exception cref="SocketException"></exception>
        public void Send(IByteRange byteRange)
        {
            if (byteRange == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            var byteRanges = byteRange.SplitBySize(BufferSetter.ARG_BUFFER_SIZE);
            foreach (var range in byteRanges)
            {
                this.SendByteRange(range);
            }
        }

        /// <summary>
        /// 发送一个小于缓冲区的数据范围
        /// </summary>
        /// <param name="byteRange">数据范围</param>
        private bool SendByteRange(IByteRange byteRange)
        {
            // 如果发送过程已停止，则本次直接发送
            if (Interlocked.CompareExchange(ref this.pendingSendCount, 1, 0) == 0)
            {
                return this.TrySendByteRangeAsync(byteRange);
            }

            // 添加数据到缓存区
            this.byteRangeQueue.Enqueue(byteRange);

            // 如果发送过程已停止，则启动发送缓存中的数据
            if (Interlocked.Increment(ref this.pendingSendCount) == 1)
            {
                return this.TrySendByteRangeAsync(null);
            }
            return true;
        }


        /// <summary>
        /// 尝试异步发送一个ByteRange
        /// 发送完成将触发SendCompleted方法
        /// <param name="byteRange">数据范围，为null则从缓冲中区获取</param>
        /// </summary>
        private bool TrySendByteRangeAsync(IByteRange byteRange)
        {
            if (byteRange == null && this.byteRangeQueue.TryDequeue(out byteRange) == false)
            {
                Interlocked.Exchange(ref this.pendingSendCount, 0);
                return false;
            }

            Buffer.BlockCopy(byteRange.Buffer, byteRange.Offset, this.sendArg.Buffer, this.sendArg.Offset, byteRange.Count);
            this.sendArg.SetBuffer(this.sendArg.Offset, byteRange.Count);

            return this.TryInvoke(() =>
            {
                if (this.socket.SendAsync(this.sendArg) == false)
                {
                    this.SendCompleted(this.socket, this.sendArg);
                }
            });
        }

        /// <summary>
        /// 发送完成时触发
        /// 将检测是否有缓存的数据要继续发送
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="arg">关联的SocketAsyncEventArgs</param>
        private void SendCompleted(object sender, SocketAsyncEventArgs arg)
        {
            if (this.socketClosed || this.IsConnected == false)
            {
                Interlocked.Exchange(ref this.pendingSendCount, 0);
            }
            else if (Interlocked.Decrement(ref this.pendingSendCount) > 0L)
            {
                this.TrySendByteRangeAsync(null);
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
                this.CloseHandler.Invoke();
            }
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

                // 清除未发送完成的数据
                if (waitForSendComplete == false && this.byteRangeQueue.Count > 0)
                {
                    this.byteRangeQueue = new ConcurrentQueue<IByteRange>();
                }

                // 自旋等待最后一次发送完成
                var spinWait = new SpinWait();
                while (this.pendingSendCount > 0)
                {
                    spinWait.SpinOnce();
                }

                try
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                }
                finally
                {
                    this.socket.Dispose();
                    this.socketClosed = true;
                }

                return true;
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
        ~TcpSession()
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
            this.sendArg.Dispose();
            this.recvArg.Dispose();

            if (disposing)
            {
                this.RecvStream = null;
                this.recvArg = null;

                this.byteRangeQueue = null;
                this.sendArg = null;

                this.socket = null;
                this.socketRoot = null;

                this.Tag = null;
            }
        }
        #endregion
    }
}

