using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Authentication;
using NetworkSocket.Http;
using NetworkSocket.Util;
using NetworkSocket.Tasks;
using System.Net.Security;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// WebSocket客户端
    /// </summary>
    public class WebSocketClient : TcpClientBase
    {
        /// <summary>
        /// 握手请求
        /// </summary>
        private readonly HandshakeRequest handshake = new HandshakeRequest(TimeSpan.FromSeconds(2));

        /// <summary>
        /// ping任务表
        /// </summary>
        private readonly TaskSetterTable<Guid> pingTable = new TaskSetterTable<Guid>();

        /// <summary>
        /// WebSocket客户端
        /// </summary>
        public WebSocketClient()
        {
        }

        /// <summary>
        /// SSL支持的WebSocket客户端
        /// </summary>
        /// <param name="targetHost">目标主机</param>
        /// <exception cref="ArgumentNullException"></exception>
        public WebSocketClient(string targetHost)
            : base(targetHost)
        {
        }

        /// <summary>
        /// SSL支持的WebSocket客户端
        /// </summary>  
        /// <param name="targetHost">目标主机</param>
        /// <param name="certificateValidationCallback">远程证书验证回调</param>
        /// <exception cref="ArgumentNullException"></exception>
        public WebSocketClient(string targetHost, RemoteCertificateValidationCallback certificateValidationCallback)
            : base(targetHost, certificateValidationCallback)
        {
        }

        /// <summary>
        /// 连接到远程终端 
        /// </summary>
        /// <param name="remoteEndPoint">远程ip和端口</param> 
        /// <exception cref="AuthenticationException"></exception>
        /// <returns></returns>
        public override sealed SocketError Connect(EndPoint remoteEndPoint)
        {
            var state = base.Connect(remoteEndPoint);
            if (state == SocketError.Success)
            {
                state = this.handshake.Execute(this);
            }

            if (state != SocketError.Success)
            {
                this.Close();
            }
            return state;
        }

        /// <summary>
        /// 连接到远程终端 
        /// </summary>
        /// <param name="remoteEndPoint">远程ip和端口</param> 
        /// <exception cref="AuthenticationException"></exception>
        /// <returns></returns>
        public override sealed async Task<SocketError> ConnectAsync(EndPoint remoteEndPoint)
        {
            var state = await base.ConnectAsync(remoteEndPoint);
            if (state == SocketError.Success)
            {
                state = await this.handshake.ExecuteAsync(this);
            }

            if (state != SocketError.Success)
            {
                this.Close();
            }
            return state;
        }


        /// <summary>
        /// 收到数据时
        /// </summary>
        /// <param name="streamReader">数据读取器</param>
        /// <returns></returns>
        protected override sealed async Task OnReceiveAsync(ISessionStreamReader streamReader)
        {
            if (this.handshake.IsWaitting == true)
            {
                this.handshake.TrySetResult(streamReader);
            }
            else
            {
                await this.OnWebSocketRequestAsync(streamReader);
            }
        }



        /// <summary>
        /// 收到请求数据
        /// </summary>
        /// <param name="streamReader">数据读取器</param>
        /// <returns></returns>
        private Task OnWebSocketRequestAsync(ISessionStreamReader streamReader)
        {
            var frames = this.GenerateWebSocketFrame(streamReader);
            foreach (var frame in frames)
            {
                this.OnFrameRequest(frame);
            }
            return TaskExtend.CompletedTask;
        }

        /// <summary>
        /// 解析生成请求帧
        /// </summary>
        /// <param name="streamReader">数据读取器</param>
        /// <returns></returns>
        private IList<FrameRequest> GenerateWebSocketFrame(ISessionStreamReader streamReader)
        {
            var list = new List<FrameRequest>();
            while (true)
            {
                try
                {
                    var request = FrameRequest.Parse(streamReader, false);
                    if (request == null)
                    {
                        return list;
                    }
                    list.Add(request);
                }
                catch (NotSupportedException ex)
                {
                    this.Close(StatusCodes.ProtocolError, ex.Message);
                    return list;
                }
            }
        }

        /// <summary>
        /// 收到请求数据
        /// </summary>
        /// <param name="frame">请求数据</param>
        private void OnFrameRequest(FrameRequest frame)
        {
            switch (frame.Frame)
            {
                case FrameCodes.Close:
                    var closeFrame = new CloseRequest(frame);
                    this.OnClose(closeFrame.StatusCode, closeFrame.CloseReason);
                    base.Close();
                    break;

                case FrameCodes.Binary:
                    this.OnBinary(frame);
                    break;

                case FrameCodes.Text: 
                    this.OnText(frame);
                    break;

                case FrameCodes.Ping:
                    var pongValue = new FrameResponse(FrameCodes.Pong, frame.Content);
                    this.TrySend(pongValue);
                    this.OnPing(frame.Content);
                    break;

                case FrameCodes.Pong:
                    this.ProcessPingPong(frame.Content);
                    this.OnPong(frame.Content);
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// 处理与ping关联
        /// </summary>
        /// <param name="pong"></param>
        private void ProcessPingPong(byte[] pong)
        {
            if (pong == null || pong.Length != 36)
            {
                return;
            }

            Guid id;
            var value = Encoding.UTF8.GetString(pong);
            if (Guid.TryParse(value, out id) == false)
            {
                return;
            }

            var setter = this.pingTable.Take(id);
            if (setter != null)
            {
                setter.SetResult(true);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private bool TrySend(FrameResponse frame)
        {
            try
            {
                return this.Send(frame) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private int Send(FrameResponse frame)
        {
            var buffer = frame.ToArraySegment(mask: true);
            return base.Send(buffer);
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>     
        /// <param name="content">文本内容</param>
        /// <exception cref="SocketException"></exception>
        public int SendText(string content)
        {
            var bytes = content == null ? new byte[0] : Encoding.UTF8.GetBytes(content);
            var text = new FrameResponse(FrameCodes.Text, bytes);
            return this.Send(text);
        }

        /// <summary>
        /// 发送二进制数据
        /// </summary>       
        /// <param name="content">二进制数据</param>
        /// <exception cref="SocketException"></exception>
        public int SendBinary(byte[] content)
        {
            var bin = new FrameResponse(FrameCodes.Binary, content);
            return this.Send(bin);
        }

        /// <summary>
        /// ping指令
        /// 服务器将回复pong
        /// </summary>
        /// <param name="contents">内容</param>
        /// <returns></returns>
        public int Ping(byte[] contents)
        {
            var ping = new FrameResponse(FrameCodes.Ping, contents);
            return this.Send(ping);
        }

        /// <summary>
        /// 向服务器ping唯一的内容
        /// 并等待匹配的回复
        /// </summary>
        /// <param name="waitTime">最多等待时间，超时则结果false</param>
        /// <returns></returns>
        public async Task<bool> PingAsync(TimeSpan waitTime)
        {
            var id = Guid.NewGuid();
            var task = this.pingTable.Create<bool>(id, waitTime);

            try
            {
                var content = Encoding.UTF8.GetBytes(id.ToString());
                this.Ping(content);
                return await task;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 正常关闭客户端
        /// </summary>       
        /// <param name="code">关闭码</param>
        public void Close(StatusCodes code)
        {
            this.Close(code, string.Empty);
        }

        /// <summary>
        /// 正常关闭客户端
        /// </summary>      
        /// <param name="code">关闭码</param>
        /// <param name="reason">原因</param>
        public void Close(StatusCodes code, string reason)
        {
            var response = new CloseResponse(code, reason);
            this.TrySend(response);
            base.Close();
        }

        /// <summary>
        /// 收到ping回复
        /// </summary>
        /// <param name="pong">pong内容</param>
        protected virtual void OnPong(byte[] pong)
        {
        }

        /// <summary>
        /// 收到服务端的ping请求
        /// </summary>
        /// <param name="ping">ping内容</param>
        protected virtual void OnPing(byte[] ping)
        {
        }

        /// <summary>
        /// 收到服务端的Text请求
        /// </summary>
        /// <param name="frame">数据帧</param>
        protected virtual void OnText(FrameRequest frame)
        {
        }

        /// <summary>
        /// 收到服务端的Binary请求
        /// </summary>
        /// <param name="frame">数据帧</param>
        protected virtual void OnBinary( FrameRequest frame)
        {
        }

        /// <summary>
        /// 收到服务端的关闭请求
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="reason">备注原因</param>
        protected virtual void OnClose(StatusCodes code, string reason)
        {
        }
    }
}
