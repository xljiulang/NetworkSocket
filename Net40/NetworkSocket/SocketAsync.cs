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
    /// Socket上下文对象
    /// 提供异步接收和发送方法
    /// </summary>
    /// <typeparam name="T">PacketBase派生类型</typeparam>
    [DebuggerDisplay("RemoteEndPoint = {RemoteEndPoint}")]
    public class SocketAsync<T> where T : PacketBase
    {
        /// <summary>
        /// socket
        /// </summary>
        private Socket socket;
        /// <summary>
        /// socket排它锁
        /// </summary>
        private object socketRoot = new object();
        /// <summary>
        /// 接收到的未处理数据
        /// </summary>
        private ByteBuilder recvBuilder = new ByteBuilder();
        /// <summary>
        /// 接收参数
        /// </summary>
        private SocketAsyncEventArgs recvEventArg = new SocketAsyncEventArgs();


        /// <summary>
        /// 发送数据的委托
        /// </summary>
        internal Action<T> SendHandler { get; set; }
        /// <summary>
        /// 处理和分析收到的数据的委托
        /// </summary>
        internal Func<ByteBuilder, T> ReceiveHandler { get; set; }
        /// <summary>
        /// 接收一个数据包委托
        /// </summary>
        internal Action<T> RecvCompleteHandler;
        /// <summary>
        /// 连接断开委托   
        /// </summary>
        internal Action DisconnectHandler;


        /// <summary>
        /// 获取动态数据字典
        /// </summary>
        public dynamic TagBag { get; private set; }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }

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
        /// 异步Socket
        /// </summary>  
        internal SocketAsync()
        {
            SocketAsyncEventArgBuffer.Instance.SetBuffer(this.recvEventArg);
            this.recvEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(this.RecvCompleted);
            this.TagBag = new TagBag();
        }

        /// <summary>
        /// 将Socket对象与此对象绑定
        /// </summary>
        /// <param name="socket">套接字</param>
        internal void BindSocket(Socket socket)
        {
            this.socket = socket;
            this.RemoteEndPoint = (IPEndPoint)this.socket.RemoteEndPoint;
            this.recvEventArg.SocketError = SocketError.Success;
            this.SetKeepAlive(socket);
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
            if (this.socket.ReceiveAsync(this.recvEventArg) == false)
            {
                this.RecvCompleted(null, this.recvEventArg);
            }
        }

        /// <summary>
        /// 将重置的未绑定Socket之前的状态
        /// 包括释放socket对象，重置相关参数
        /// 如果已重置过，将返回false
        /// </summary>
        /// <returns></returns>
        internal bool CloseSocket()
        {
            lock (this.socketRoot)
            {
                if (this.socket == null)
                {
                    return false;
                }

                try
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Dispose();
                }
                finally
                {
                    this.socket = null;
                }
                // 关闭socket前重置相关数据               
                this.recvBuilder.Clear();
                (this.TagBag as TagBag).Clear();
                this.RemoteEndPoint = null;
                return true;
            }
        }


        /// <summary>
        /// 接收到数据事件
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="eventArg">参数</param>
        private void RecvCompleted(object sender, SocketAsyncEventArgs eventArg)
        {
            if (eventArg.BytesTransferred == 0 || eventArg.SocketError != SocketError.Success)
            {
                this.DisconnectHandler();
                return;
            }

            lock (this.recvBuilder.SyncRoot)
            {
                T packet = null;
                this.recvBuilder.Add(eventArg.Buffer, eventArg.Offset, eventArg.BytesTransferred);
                while ((packet = this.ReceiveHandler(this.recvBuilder)) != null)
                {
                    this.RecvCompleteHandler(packet);
                }
            }

            lock (this.socketRoot)
            {
                if (this.socket != null && this.socket.ReceiveAsync(eventArg) == false)
                {
                    this.RecvCompleted(null, eventArg);
                }
            }
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="packet">数据包</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception>       
        public void Send(T packet)
        {
            this.SendHandler(packet);

            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            if (this.IsConnected == false)
            {
                throw new SocketException();
            }

            var bytes = packet.ToByteArray();
            if (bytes == null)
            {
                throw new ArgumentException("packet");
            }

            this.Send(bytes);
        }


        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="bytes">数据</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SocketException"></exception>
        private void Send(byte[] bytes)
        {
            var eventArg = SocketAsyncEventArgPool.Instance.Take();
            eventArg.SetBuffer(bytes, 0, bytes.Length);
            if (this.socket.SendAsync(eventArg) == false)
            {
                SocketAsyncEventArgPool.Instance.Add(eventArg);
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
        ~SocketAsync()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            this.CloseSocket();
            this.recvEventArg.Dispose();

            if (disposing)
            {
                this.RemoteEndPoint = null;
                this.recvBuilder = null;
                this.TagBag = null;
                this.socketRoot = null;
            }
        }
        #endregion
    }
}

