using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace NetworkSocket
{
    /// <summary>
    /// 会话对象基础类  
    /// 所有会话对象和客户端都派生于此类
    /// </summary>        
    [DebuggerDisplay("{RemoteEndPoint}")]
    public class SessionBase : ISession, IDisposable
    {
        /// <summary>
        /// socket
        /// </summary>
        private volatile Socket socket;

        /// <summary>
        /// socket同步锁
        /// </summary>
        private object socketRoot = new object();

        /// <summary>
        /// 是否已关闭
        /// </summary>
        private bool closed = true;

        /// <summary>
        /// 接收到的未处理数据
        /// </summary>
        private ReceiveBuffer recvBuffer = new ReceiveBuffer(Endians.Big);

        /// <summary>
        /// 接收参数
        /// </summary>
        private SocketAsyncEventArgs recvArg = new SocketAsyncEventArgs();


        /// <summary>
        /// 处理和分析收到的数据的委托
        /// </summary>
        internal Action<ReceiveBuffer> ReceiveHandler;

        /// <summary>
        /// 连接断开委托   
        /// </summary>
        internal Action DisconnectHandler;

        /// <summary>
        /// 关闭时的委托
        /// </summary>
        internal Action CloseHandler;


        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// 获取用户附加数据
        /// 与TagData共享
        /// </summary>
        public dynamic TagBag { get; private set; }

        /// <summary>
        /// 获取用户附加数据
        /// 与TagBag共享
        /// </summary>
        public ITag TagData { get; private set; }


        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock (this.socketRoot)
                {
                    return this.socket != null && this.socket.Connected;
                }
            }
        }

        /// <summary>
        /// Socket客户端
        /// </summary>  
        public SessionBase()
        {
            this.recvArg.Completed += new EventHandler<SocketAsyncEventArgs>(this.RecvCompleted);
            this.TagData = new TagData();
            this.TagBag = new TagBag((TagData)this.TagData);

            RecvArgBuffer.SetBuffer(this.recvArg);
        }

        /// <summary>
        /// 绑定一个Socket对象
        /// </summary>
        /// <param name="socket">套接字</param>
        internal void Bind(Socket socket)
        {
            this.socket = socket;
            this.recvArg.SocketError = SocketError.Success;
            this.recvBuffer.Clear();
            this.TagData.Clear();
            this.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            this.SetKeepAlive(socket);
            this.closed = false;
        }

        /// <summary>
        /// 设置客户端的心跳包
        /// </summary>
        /// <param name="socket">客户端</param>
        private void SetKeepAlive(Socket socket)
        {
            var inOptionValue = new byte[12];
            var outOptionValue = new byte[12];

            ByteConverter.ToBytes(1, Endians.Little).CopyTo(inOptionValue, 0);
            ByteConverter.ToBytes(5 * 1000, Endians.Little).CopyTo(inOptionValue, 4);
            ByteConverter.ToBytes(5 * 1000, Endians.Little).CopyTo(inOptionValue, 8);

            try
            {
                socket.IOControl(IOControlCode.KeepAliveValues, inOptionValue, outOptionValue);
            }
            catch (NotSupportedException)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, inOptionValue);
            }
            catch (NotImplementedException)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, inOptionValue);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        internal void BeginReceive()
        {
            lock (this.socketRoot)
            {
                if (this.socket != null && this.socket.ReceiveAsync(this.recvArg) == false)
                {
                    this.RecvCompleted(null, this.recvArg);
                }
            }
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

            lock (this.recvBuffer.SyncRoot)
            {
                this.recvBuffer.Add(arg.Buffer, arg.Offset, arg.BytesTransferred);
            }
            this.ReceiveHandler(this.recvBuffer);
            // 重新进行一次接收
            this.BeginReceive();
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>  
        /// <exception cref="ArgumentNullException"></exception>        
        /// <exception cref="SocketException"></exception>
        public void Send(ByteRange byteRange)
        {
            if (byteRange == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            var sendArg = SendArgBag.Take();
            sendArg.SetBuffer(byteRange.Buffer, byteRange.Offset, byteRange.Count);

            if (this.socket.SendAsync(sendArg) == false)
            {
                SendArgBag.Add(sendArg);
            }
        }

        /// <summary>
        /// 断开和远程端的连接             
        /// </summary>
        /// <returns></returns>
        public virtual void Close()
        {
            lock (this.socketRoot)
            {
                if (this.closed == true)
                {
                    return;
                }

                try
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Dispose();
                }
                finally
                {
                    socket = null;
                    this.closed = true;
                }

                if (this.CloseHandler != null)
                {
                    this.CloseHandler();
                }
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
        void IDisposable.Dispose()
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
        ~SessionBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            this.Close();
            this.recvArg.Dispose();

            if (disposing)
            {
                this.recvBuffer = null;
                this.recvArg = null;
                this.socket = null;
                this.socketRoot = null;

                this.TagBag = null;
                this.TagData = null;
            }
        }
        #endregion
    }
}

