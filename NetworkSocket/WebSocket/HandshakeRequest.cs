using NetworkSocket.Http;
using NetworkSocket.Tasks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示握手请求
    /// </summary>
    public sealed class HandshakeRequest
    {
        /// <summary>
        /// 安全key
        /// </summary>
        private string secKey;

        /// <summary>
        /// 超时时间
        /// </summary>
        private readonly TimeSpan timeout;

        /// <summary>
        /// 任务行为
        /// </summary>
        private ITaskSetter<SocketError> taskSetter;


        /// <summary>
        /// 是否正在等待回复
        /// </summary>
        private long waitiing = 0L;


        /// <summary>
        /// 获取是否正在等待回复
        /// </summary>
        public bool IsWaitting
        {
            get
            {
                return Interlocked.Read(ref this.waitiing) > 0L;
            }
            private set
            {
                var wait = value ? 1L : 0L;
                Interlocked.Exchange(ref this.waitiing, wait);
            }
        }

        /// <summary>
        /// 超时时间
        /// </summary>
        /// <param name="timeout">时间</param>
        public HandshakeRequest(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        /// <summary>
        /// 异步执行握手请求
        /// </summary>     
        /// <param name="client">客户端</param>
        /// <param name="path">请求路径</param>
        /// <returns></returns>
        public Task<SocketError> ExecuteAsync(WebSocketClient client, string path)
        {
            try
            {
                this.IsWaitting = true;
                this.taskSetter = new TaskSetter<SocketError>()
                    .TimeoutAfter(this.timeout, (self) => self.SetResult(SocketError.TimedOut));

                var handshakeBuffer = this.GenerateHandshakeBuffer(client, path, out this.secKey);
                client.Send(handshakeBuffer);
            }
            catch (SocketException ex)
            {
                this.TrySetResult(ex.SocketErrorCode);
            }
            catch (Exception)
            {
                this.TrySetResult(SocketError.SocketError);
            }
            return this.taskSetter.GetTask();
        }

        /// <summary>
        /// 执行握手请求
        /// </summary>     
        /// <param name="client">客户端</param>
        /// <param name="path">请求路径</param>
        /// <returns></returns>
        public SocketError Execute(WebSocketClient client, string path)
        {
            try
            {
                this.IsWaitting = true;
                this.taskSetter = new TaskSetter<SocketError>().TimeoutAfter(this.timeout);
                var handshakeBuffer = this.GenerateHandshakeBuffer(client, path, out this.secKey);
                client.Send(handshakeBuffer);
            }
            catch (SocketException ex)
            {
                this.TrySetResult(ex.SocketErrorCode);
            }
            catch (Exception)
            {
                this.TrySetResult(SocketError.SocketError);
            }

            return Task.Run(() => this.taskSetter.GetResult()).Result;
        }

        /// <summary>
        /// 设置握手结果
        /// </summary>
        /// <param name="result">结果值</param>
        /// <returns></returns>
        private bool TrySetResult(SocketError result)
        {
            this.IsWaitting = false;
            return this.taskSetter.SetResult(result);
        }

        /// <summary>
        /// 设置握手结果
        /// </summary>
        /// <param name="streamReader">数据读取器</param>
        /// <returns></returns>
        public bool TrySetResult(ISessionStreamReader streamReader)
        {
            var result = HttpResponseParser.Parse(streamReader);
            if (result.IsHttp == false)
            {
                return false;
            }
            else
            {
                streamReader.Clear();
            }

            if (result.Status == 101)
            {
                const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                var bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(this.secKey + guid));
                var secValue = Convert.ToBase64String(bytes);
                var secAccept = result.Header["Sec-WebSocket-Accept"];
                if (secValue == secAccept)
                {
                    return this.TrySetResult(SocketError.Success);
                }
            }

            return this.TrySetResult(SocketError.SocketError);
        }


        /// <summary>
        /// 生成握手内容
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="path">路径</param>
        /// <param name="secKey">安全Key</param>
        /// <returns></returns>
        private byte[] GenerateHandshakeBuffer(WebSocketClient client, string path, out string secKey)
        {
            var host = client.RemoteEndPoint.ToString();
            if (client.RemoteEndPoint is DnsEndPoint dnsEndpoint)
            {
                host = string.Format("{0}:{1}", dnsEndpoint.Host, dnsEndpoint.Port);
            }

            var keyBytes = SHA1.Create().ComputeHash(Guid.NewGuid().ToByteArray());
            secKey = Convert.ToBase64String(keyBytes);

            var header = HeaderBuilder.NewRequest(HttpMethod.GET, path);
            header.Add("Host", host);
            header.Add("Connection", "Upgrade");
            header.Add("Upgrade", "websocket");
            header.Add("Origin", "http://" + host);
            header.Add("Sec-WebSocket-Version", "13");
            header.Add("Sec-WebSocket-Key", this.secKey);
            return header.ToByteArray();
        }
    }
}
