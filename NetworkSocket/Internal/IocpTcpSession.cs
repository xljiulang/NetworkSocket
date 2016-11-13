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

            try
            {
                if (this.Socket.ReceiveAsync(this.recvArg) == false)
                {
                    this.RecvCompleted(this.Socket, this.recvArg);
                }
            }
            catch (Exception)
            {
                this.DisconnectHandler(this);
            }
        }

        /// <summary>
        /// 接收到数据事件
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="arg">参数</param>
        private async void RecvCompleted(object sender, SocketAsyncEventArgs arg)
        {
            if (arg.BytesTransferred == 0 || arg.SocketError != SocketError.Success)
            {
                this.DisconnectHandler(this);
                return;
            }

            lock (this.StreamReader.SyncRoot)
            {
                this.StreamReader.Stream.Seek(0, SeekOrigin.End);
                this.StreamReader.Stream.Write(arg.Buffer, arg.Offset, arg.BytesTransferred);
                this.StreamReader.Stream.Seek(0, SeekOrigin.Begin);
            }

            // 重新进行一次接收
            await this.ReceiveAsyncHandler(this);
            this.TryReceiveAsync();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.recvArg.Dispose();

            if (disposing == true)
            {
                this.recvArg = null;
            }
        }
    }
}

