using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkSocket.Interfaces;

namespace NetworkSocket
{
    /// <summary>
    /// Tcp客户端抽象类
    /// 所有Tcp客户端都派生于此类
    /// </summary>
    /// <typeparam name="T">PacketBase派生类型</typeparam>
    public abstract class TcpClientBase<T> : SocketAsync<T>, ITcpClient<T> where T : PacketBase
    {
        /// <summary>
        /// Tcp客户端抽象类
        /// </summary>
        public TcpClientBase()
        {
            base.SendHandler = this.OnSend;
            base.ReceiveHandler = this.OnReceive;
            base.RecvCompleteHandler = this.OnRecvComplete;
            base.DisconnectHandler = this.OnDisconnect;
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
            connectArg.Completed += this.ConnectArg_Completed;

            if (socket.ConnectAsync(connectArg) == false)
            {
                this.ConnectArg_Completed(socket, connectArg);
            }
            return taskSource.Task;
        }

        /// <summary>
        /// 连接完成事件
        /// </summary>
        /// <param name="sender">连接者</param>
        /// <param name="e">事件参数</param>
        private void ConnectArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            var socket = sender as Socket;
            var taskSource = e.UserToken as TaskCompletionSource<bool>;
            var result = e.SocketError == SocketError.Success;

            if (result == true)
            {
                base.BindSocket(socket);
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
        /// 当接收到远程端的数据时，将触发此方法
        /// 此方法用于处理和分析收到的数据
        /// 如果得到一个数据包，将触发OnRecvComplete方法
        /// [注]这里只需处理一个数据包的流程
        /// </summary>
        /// <param name="recvBuilder">接收到的历史数据</param>
        /// <returns>如果不够一个数据包，则请返回null</returns>
        protected abstract T OnReceive(ByteBuilder recvBuilder);

        /// <summary>
        /// 发送之前触发
        /// </summary>
        /// <param name="packet">数据包</param>
        protected virtual void OnSend(T packet)
        {
        }

        /// <summary>
        /// 当接收到数据包，将触发此方法
        /// </summary>
        /// <param name="packet">数据包</param>
        protected virtual void OnRecvComplete(T packet)
        {
        }

        /// <summary>
        /// 当与服务器断开连接时，将触发此方法
        /// </summary>       
        protected virtual void OnDisconnect()
        {
        }

        /// <summary>
        /// 断开和远程终端的连接
        /// </summary>
        public void Close()
        {
            base.CloseSocket();
        }
    }
}