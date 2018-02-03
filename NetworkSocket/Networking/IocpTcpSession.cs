using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 表示IOCP的Tcp会话对象  
    /// </summary>        
    class IocpTcpSession : TcpSessionBase
    {
        /// <summary>
        /// 用于接收的SocketAsyncEventArgs
        /// </summary>
        private readonly SocketAsyncEventArgs recvArg = new SocketAsyncEventArgs();

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
        public override void SetSocket(Socket socket)
        {
            this.recvArg.SocketError = SocketError.Success;
            base.SetSocket(socket);
        }

        /// <summary>
        /// 异步接收数据
        /// 将接收结果写入StreamReader
        /// 如果返回false表示接收异常
        /// </summary>
        /// <returns></returns>
        protected override async Task<bool> ReceiveAsync()
        {
            try
            {
                var taskSource = new TaskCompletionSource<bool>();
                this.recvArg.UserToken = taskSource;

                if (this.Socket.ReceiveAsync(this.recvArg))
                {
                    return await taskSource.Task;
                }
                else
                {
                    return await this.ProcessReceiveAsync(this.recvArg);
                }
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
        private void OnReceiveAsynCompleted(object sender, SocketAsyncEventArgs arg)
        {
            // 切换到工作线程处理业务逻辑
            ThreadPool.QueueUserWorkItem(async state =>
            {
                var taskSource = arg.UserToken as TaskCompletionSource<bool>;
                var result = await this.ProcessReceiveAsync(arg);
                taskSource.TrySetResult(result);
            }, null);
        }

        /// <summary>
        /// 异步处理接收到数据
        /// </summary>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        private async Task<bool> ProcessReceiveAsync(SocketAsyncEventArgs arg)
        {
            if (arg.BytesTransferred == 0 || arg.SocketError != SocketError.Success)
            {
                var handler = this.DisconnectHandler;
                handler?.Invoke(this);
                return false;
            }

            lock (this.StreamReader.SyncRoot)
            {
                this.StreamReader.Stream.Seek(0, SeekOrigin.End);
                this.StreamReader.Stream.Write(arg.Buffer, arg.Offset, arg.BytesTransferred);
                this.StreamReader.Stream.Seek(0, SeekOrigin.Begin);
            }

            var recvedHandler = this.ReceiveCompletedHandler;
            if (recvedHandler != null)
            {
                await recvedHandler(this);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.recvArg.Dispose();
        }
    }
}

