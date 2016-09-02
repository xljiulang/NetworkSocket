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
    /// 表示IOCP的Tcp会话对象  
    /// </summary>        
    internal sealed class IocpTcpSession : TcpSessionBase
    {
        /// <summary>
        /// 用于发送的SocketAsyncEventArgs
        /// </summary>
        private SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();

        /// <summary>
        /// 用于接收的SocketAsyncEventArgs
        /// </summary>
        private SocketAsyncEventArgs recvArg = new SocketAsyncEventArgs();

        /// <summary>
        /// 获取会话是否提供SSL/TLS安全
        /// </summary>
        public override bool IsSecurity
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// IOCP的Tcp会话对象  
        /// </summary>  
        public IocpTcpSession()
        {
            this.sendArg.Completed += this.SendCompleted;
            this.recvArg.Completed += this.RecvCompleted;
            BufferManager.SetBuffer(this.recvArg);
        }

        /// <summary>
        /// 绑定一个Socket对象
        /// </summary>
        /// <param name="socket">套接字</param>
        public override void Bind(Socket socket)
        {
            this.recvArg.SocketError = SocketError.Success;
            this.sendArg.SocketError = SocketError.Success;
            base.Bind(socket);
        }

        /// <summary>
        /// 开始循环接收数据 
        /// </summary>
        public override void LoopReceive()
        {
            this.TryReceiveAsync();
        }

        /// <summary>
        /// 尝试开始接收数据
        /// </summary>
        private void TryReceiveAsync()
        {
            if (this.IsConnected == false)
            {
                return;
            }

            base.TryInvokeAction(() =>
            {
                if (this.Socket.ReceiveAsync(this.recvArg) == false)
                {
                    this.RecvCompleted(this.Socket, this.recvArg);
                }
            }, () => this.DisconnectHandler(this));
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
                this.DisconnectHandler(this);
                return;
            }

            lock (this.RecvStream.SyncRoot)
            {
                this.RecvStream.Seek(0, SeekOrigin.End);
                this.RecvStream.Write(arg.Buffer, arg.Offset, arg.BytesTransferred);
                this.RecvStream.Seek(0, SeekOrigin.Begin);
                this.ReceiveHandler(this);
            }

            // 重新进行一次接收
            this.TryReceiveAsync();
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>
        /// <exception cref="ArgumentNullException"></exception>        
        /// <exception cref="SocketException"></exception>
        public override void Send(IByteRange byteRange)
        {
            if (byteRange == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            this.Socket.Send(byteRange.Buffer, byteRange.Offset, byteRange.Count, SocketFlags.None);
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>  
        /// <exception cref="ArgumentNullException"></exception>        
        /// <exception cref="SocketException"></exception>
        public override void SendAsync(IByteRange byteRange)
        {
            if (byteRange == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            // 如果发送过程已停止，则本次直接发送
            if (Interlocked.CompareExchange(ref this.PendingSendCount, 1, 0) == 0)
            {
                this.TrySendByteRangeAsync(byteRange);
            }
            else
            {
                this.PendingSendByteRanges.Enqueue(byteRange);
                // 如果发送过程已停止，则启动发送缓存中的数据
                if (Interlocked.Increment(ref this.PendingSendCount) == 1)
                {
                    this.TrySendByteRangeAsync(null);
                }
            }
        }


        /// <summary>
        /// 尝试异步发送一个ByteRange
        /// 发送完成将触发SendCompleted方法
        /// <param name="byteRange">数据范围，为null则从缓冲中区获取</param>
        /// </summary>
        private void TrySendByteRangeAsync(IByteRange byteRange)
        {
            if (byteRange == null && this.PendingSendByteRanges.TryDequeue(out byteRange) == false)
            {
                Interlocked.Exchange(ref this.PendingSendCount, 0);
                return;
            }

            base.TryInvokeAction(() =>
            {
                this.sendArg.SetBuffer(byteRange.Buffer, byteRange.Offset, byteRange.Count);
                if (this.Socket.SendAsync(this.sendArg) == false)
                {
                    this.SendCompleted(this.Socket, this.sendArg);
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
            if (this.IsConnected == false)
            {
                Interlocked.Exchange(ref this.PendingSendCount, 0);
            }
            else if (Interlocked.Decrement(ref this.PendingSendCount) > 0)
            {
                this.TrySendByteRangeAsync(null);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.sendArg.Dispose();
            this.recvArg.Dispose();

            if (disposing == true)
            {
                this.recvArg = null;
                this.sendArg = null;
            }
        }      
    }
}

