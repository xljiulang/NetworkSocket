using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;

namespace NetworkSocket
{
    /// <summary>
    /// Tcp客户端抽象类
    /// 所有Tcp客户端都派生于此类
    /// </summary>   
    public abstract class TcpClientBase : SessionBase, ITcpClient
    {
        /// <summary>
        /// Tcp客户端抽象类
        /// </summary>
        public TcpClientBase()
        {
            base.ReceiveHandler = this.OnReceive;
            base.DisconnectHandler = this.Disconnect;
        }

        /// <summary>
        /// 连接到远程端
        /// </summary>
        /// <param name="hostNameOrAddress">域名或ip地址</param>
        /// <param name="port">远程端口</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public Task<bool> Connect(string hostNameOrAddress, int port)
        {
            var ipAddress = Dns.GetHostAddresses(hostNameOrAddress);
            return this.Connect(ipAddress.Last(), port);
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
                base.TryReceive();
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
        /// </summary>       
        /// <param name="buffer">接收到的历史数据</param>
        /// <returns></returns>
        protected abstract void OnReceive(ReceiveBuffer buffer);


        /// <summary>
        /// 关闭连接并触发关闭事件
        /// </summary>
        private void Disconnect()
        {
            this.Close();
            this.OnDisconnect();
        }

        /// <summary>
        /// 当与服务器断开连接时，将触发此方法
        /// </summary>       
        protected virtual void OnDisconnect()
        {
        }
    }
}