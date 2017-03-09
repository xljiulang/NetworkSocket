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
using System.Threading.Tasks;
using NetworkSocket.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 表示IOCP的Tcp会话对象  
    /// </summary>        
    internal class IocpTcpSession : TcpSessionBase
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
            this.recvArg.Completed += OnReceiveAsynCompleted;
            BufferPool.SetBuffer(this.recvArg);
        }


        /// <summary>
        /// 绑定一个Socket对象
        /// </summary>
        /// <param name="socket">套接字</param>
        public override void BindSocket(Socket socket)
        {
            this.recvArg.SocketError = SocketError.Success;
            base.BindSocket(socket);
        }

        /// <summary>
        /// 异步接收数据
        /// 将接收结果写入StreamReader
        /// 如果返回false表示接收异常
        /// </summary>
        /// <returns></returns>
        protected override async Task<bool> ReceiveTaskAsync()
        {
            try
            {
                var taskSource = new TaskCompletionSource<bool>();
                this.recvArg.UserToken = taskSource;

                if (this.Socket.ReceiveAsync(this.recvArg))
                {
                    await taskSource.Task;
                }
                else
                {
                    await this.OnReceiveCompleted(this.recvArg);
                }
                return true;
            }
            catch (Exception)
            {
                this.DisconnectHandler(this);
                return false;
            }
        }

        /// <summary>
        /// 异步接收到数据
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        private async void OnReceiveAsynCompleted(object sender, SocketAsyncEventArgs arg)
        {
            var taskSource = arg.UserToken as TaskCompletionSource<bool>;
            var result = await this.OnReceiveCompleted(arg);
            taskSource.TrySetResult(result);
        }


        /// <summary>
        /// 同步接收到数据
        /// </summary>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        private async Task<bool> OnReceiveCompleted(SocketAsyncEventArgs arg)
        {
            if (arg.BytesTransferred == 0 || arg.SocketError != SocketError.Success)
            {
                this.DisconnectHandler(this);
                return false;
            }

            lock (this.StreamReader.SyncRoot)
            {
                this.StreamReader.Stream.Seek(0, SeekOrigin.End);
                this.StreamReader.Stream.Write(arg.Buffer, arg.Offset, arg.BytesTransferred);
                this.StreamReader.Stream.Seek(0, SeekOrigin.Begin);
            }

            await this.ReceiveAsyncHandler(this);
            return true;
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

