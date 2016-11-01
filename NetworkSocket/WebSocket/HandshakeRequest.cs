using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示握手请求
    /// </summary>
    public class HandshakeRequest
    {
        /// <summary>
        /// 换行
        /// </summary>
        private static readonly string CRLF = "\r\n";

        /// <summary>
        /// 获取双换行
        /// </summary>
        private static readonly byte[] DoubleCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");


        /// <summary>
        /// 定时器
        /// </summary>
        private Timer timer;

        /// <summary>
        /// 安全key
        /// </summary>
        private string secKey;

        /// <summary>
        /// 超时时间
        /// </summary>
        private readonly TimeSpan timeout;


        /// <summary>
        /// 任务源
        /// </summary>
        private TaskCompletionSource<SocketError> taskSource;


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
        /// 生成握手内容
        /// </summary>
        /// <param name="client">客户端</param>
        /// <returns></returns>
        protected string GenerateHandshakeContent(WebSocketClient client)
        {
            var host = client.RemoteEndPoint.ToString();
            var dnsEndpoint = client.RemoteEndPoint as DnsEndPoint;
            if (dnsEndpoint != null)
            {
                host = string.Format("{0}:{1}", dnsEndpoint.Host, dnsEndpoint.Port);
            }

            var guid = Guid.NewGuid().ToByteArray();
            var bytes = SHA1.Create().ComputeHash(guid);
            this.secKey = Convert.ToBase64String(bytes);

            var builder = new StringBuilder()
                .AppendFormat("GET / HTTP/1.1").Append(CRLF)
                .AppendFormat("{0}: {1}", "Host", host).Append(CRLF)
                .AppendFormat("{0}: {1}", "Connection", "Upgrade").Append(CRLF)
                .AppendFormat("{0}: {1}", "Upgrade", "websocket").Append(CRLF)
                .AppendFormat("{0}: {1}", "Origin", "http://" + host).Append(CRLF)
                .AppendFormat("{0}: {1}", "Sec-WebSocket-Version", "13").Append(CRLF)
                .AppendFormat("{0}: {1}", "Sec-WebSocket-Key", this.secKey).Append(CRLF)
                .Append(CRLF);

            return builder.ToString();
        }

        /// <summary>
        /// 执行握手请求
        /// </summary>
        /// <param name="client">客户端</param>
        /// <returns></returns>
        public SocketError Execute(WebSocketClient client)
        {
            var task = this.ExecuteAsync(client);
            if (task.Wait(this.timeout) == false)
            {
                this.timer.Dispose();
                return SocketError.TimedOut;
            }
            return task.Result;
        }

        /// <summary>
        /// 异步执行握手请求
        /// </summary>     
        /// <param name="client">客户端</param>
        /// <returns></returns>
        public Task<SocketError> ExecuteAsync(WebSocketClient client)
        {
            try
            {
                this.IsWaitting = true;
                this.taskSource = new TaskCompletionSource<SocketError>();
                var content = this.GenerateHandshakeContent(client);
                var buffer = Encoding.ASCII.GetBytes(content);
                client.Send(buffer);
                this.timer = new Timer((state) => this.TrySetResult(SocketError.TimedOut), null, this.timeout, Timeout.InfiniteTimeSpan);
            }
            catch (SocketException ex)
            {
                this.TrySetResult(ex.SocketErrorCode);
            }
            catch (Exception)
            {
                this.TrySetResult(SocketError.SocketError);
            }
            return this.taskSource.Task;
        }

        /// <summary>
        /// 设置握手结果
        /// </summary>
        /// <param name="result">结果值</param>
        /// <returns></returns>
        public bool TrySetResult(SocketError result)
        {
            this.IsWaitting = false;
            this.timer.Dispose();
            return this.taskSource.TrySetResult(result);
        }


        /// <summary>
        /// 设置握手结果
        /// </summary>
        /// <param name="inputStream">输入流</param>
        /// <returns></returns>
        public bool TrySetResult(IStreamReader inputStream)
        {
            inputStream.Position = 0;
            var index = inputStream.IndexOf(DoubleCrlf);
            if (index < 0)
            {
                return false;
            }

            var length = index + DoubleCrlf.Length;
            var header = inputStream.ReadString(Encoding.ASCII, length);
            inputStream.Clear(length);

            const string pattern = @"^HTTP/1.1 101 Switching Protocols\r\n((?<field_name>[^:\r\n]+):\s(?<field_value>[^\r\n]*)\r\n)+\r\n";
            var match = Regex.Match(header, pattern, RegexOptions.IgnoreCase);
            if (match.Success == true)
            {
                var httpHeader = HttpHeader.Parse(match.Groups["field_name"].Captures, match.Groups["field_value"].Captures);
                var secAccept = httpHeader["Sec-WebSocket-Accept"];

                const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                var bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(this.secKey + guid));
                var secValue = Convert.ToBase64String(bytes);

                if (secValue == secAccept)
                {
                    return this.TrySetResult(SocketError.Success);
                }
            }
            return this.TrySetResult(SocketError.SocketError);
        }
    }
}
