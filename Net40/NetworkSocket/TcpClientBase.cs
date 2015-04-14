using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// Tcp客户端抽象类
    /// 所有Tcp客户端都派生于此类
    /// </summary>
    /// <typeparam name="T">发送数据包协议</typeparam>
    /// <typeparam name="TRecv">接收到的数据包类型</typeparam>
    public abstract class TcpClientBase<T, TRecv> : SocketClient<T, TRecv>, ITcpClient<T>
        where T : PacketBase
        where TRecv : class
    {
        /// <summary>
        /// Tcp客户端抽象类
        /// </summary>
        public TcpClientBase()
        {
            base.ReceiveHandler = this.OnReceive;
            base.RecvCompleteHandler = this.OnRecvCompleteHandleWithTask;
            base.DisconnectHandler = this.CloseAndRaiseDisconnect;
            base.SendHandler = this.OnSend;
        }

        /// <summary>
        /// 连接到远程终端       
        /// </summary>
        /// <param name="ip">远程ip</param>
        /// <param name="port">远程端口</param>
        /// <returns></returns>
        public Task<bool> Connect(IPAddress ip, int port)
        {
            return this.Connect(new IPEndPoint(ip, port));
        }

        /// <summary>
        /// 连接到远程终端 
        /// </summary>
        /// <param name="remoteEndPoint">远程ip和端口</param> 
        /// <returns></returns>
        public Task<bool> Connect(IPEndPoint remoteEndPoint)
        {
            var taskSource = new TaskCompletionSource<bool>();
            if (this.IsConnected)
            {
                taskSource.SetResult(true);
                return taskSource.Task;
            }

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var connectArg = new SocketAsyncEventArgs { RemoteEndPoint = remoteEndPoint, UserToken = taskSource };
            connectArg.Completed += this.ConnectArgCompleted;

            if (socket.ConnectAsync(connectArg) == false)
            {
                this.ConnectArgCompleted(socket, connectArg);
            }
            return taskSource.Task;
        }

        /// <summary>
        /// 连接完成事件
        /// </summary>
        /// <param name="sender">连接者</param>
        /// <param name="e">事件参数</param>
        private void ConnectArgCompleted(object sender, SocketAsyncEventArgs e)
        {
            var socket = sender as Socket;
            var taskSource = e.UserToken as TaskCompletionSource<bool>;
            var result = e.SocketError == SocketError.Success;

            if (result == true)
            {
                base.Bind(socket);
                base.BeginReceive();
            }
            else
            {
                socket.Dispose();
            }

            e.Dispose();
            taskSource.SetResult(result);
        }

        /// <summary>
        /// 当前与远程端连接断开之后，进行重新连接   
        /// 如果还连接，则返回TaskOf(false)
        /// </summary>
        /// <returns></returns>
        public Task<bool> ReConnect()
        {
            if (this.IsConnected == true)
            {
                return Task.Factory.StartNew(() => false);
            }
            return this.Connect(this.RemoteEndPoint);
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法
        /// 此方法用于处理和分析收到的数据
        /// 如果得到一个数据包，将触发OnRecvComplete方法
        /// [注]这里只需处理一个数据包的流程
        /// </summary>
        /// <param name="builder">接收到的历史数据</param>
        /// <returns>如果不够一个数据包，则请返回null</returns>
        protected abstract TRecv OnReceive(ByteBuilder builder);


        /// <summary>
        /// 使用Task来处理OnRecvComplete业务方法
        /// 重写此方法，使用LimitedTask来代替系统默认的Task可以控制并发数
        /// </summary>       
        /// <param name="tRecv">接收到的数据类型</param>
        protected virtual void OnRecvCompleteHandleWithTask(TRecv tRecv)
        {
            Task.Factory.StartNew(() => this.OnRecvComplete(tRecv));
        }

        /// <summary>
        /// 关闭连接并触发关闭事件
        /// </summary>
        private void CloseAndRaiseDisconnect()
        {
            this.Close();
            this.OnDisconnect();
        }

        /// <summary>
        /// 当接收到数据包，将触发此方法
        /// </summary>
        /// <param name="tRecv">接收到的数据类型</param>
        protected virtual void OnRecvComplete(TRecv tRecv)
        {
        }

        /// <summary>
        /// 当与服务器断开连接时，将触发此方法
        /// </summary>       
        protected virtual void OnDisconnect()
        {
        }

        /// <summary>
        /// 发送数据包后触发
        /// </summary>       
        /// <param name="packet">数据包</param>
        protected virtual void OnSend(T packet)
        {
        }
    }
}