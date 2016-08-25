using NetworkSocket.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace NetworkSocket
{
    /// <summary>
    /// 表示SSL的Tcp会话对象  
    /// </summary>        
    internal sealed class SslTcpSession : TcpSessionBase
    {
        /// <summary>
        /// 目标主机
        /// </summary>
        private string targetHost;

        /// <summary>
        /// SSL数据流
        /// </summary>
        private SslStream sslStream;

        /// <summary>
        /// 服务器证书
        /// </summary>
        private X509Certificate certificate;

        /// <summary>
        /// 远程证书验证回调
        /// </summary>
        private RemoteCertificateValidationCallback certificateValidationCallback;

        /// <summary>
        /// 缓冲区范围
        /// </summary>
        private IByteRange bufferRange = BufferManager.GetBuffer();

        /// <summary>
        /// 获取会话是否提供SSL/TLS安全
        /// </summary>
        public override bool IsSecurity
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 表示SSL服务器会话对象
        /// </summary>  
        /// <param name="certificate">服务器证书</param>
        ///  <exception cref="ArgumentNullException"></exception>
        public SslTcpSession(X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException();
            }
            this.certificate = certificate;
            this.certificateValidationCallback = (a, b, c, d) => true;
        }

        /// <summary>
        /// 表示SSL客户端会话对象
        /// </summary>  
        /// <param name="targetHost">目标主机</param>
        /// <param name="certificateValidationCallback">远程证书验证回调</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SslTcpSession(string targetHost, RemoteCertificateValidationCallback certificateValidationCallback)
        {
            if (string.IsNullOrEmpty(targetHost) == true)
            {
                throw new ArgumentNullException("targetHost");
            }
            this.targetHost = targetHost;
            this.certificateValidationCallback = certificateValidationCallback;
        }

        /// <summary>
        /// 绑定一个Socket对象
        /// </summary>
        /// <param name="socket">套接字</param>
        public override void Bind(Socket socket)
        {
            var nsStream = new NetworkStream(socket, false);
            this.sslStream = new SslStream(nsStream, false, this.certificateValidationCallback);
            base.Bind(socket);
        }

        /// <summary>
        /// 开始循环接收数据
        /// </summary>
        /// <exception cref="AuthenticationException"></exception>
        public override void LoopReceive()
        {
            if (this.certificate == null)
            {
                this.sslStream.AuthenticateAsClient(this.targetHost);
                this.TryBeginRead();
            }
            else
            {
                base.TryInvokeAction(() =>
                    this.sslStream.BeginAuthenticateAsServer(
                    this.certificate,
                    this.EndAuthenticateAsServer,
                    null), ((ISession)this).Close);
            }
        }

        /// <summary>
        /// 服务器验证完成后
        /// </summary>
        /// <param name="asyncResult">异步结果</param>
        private void EndAuthenticateAsServer(IAsyncResult asyncResult)
        {
            var result = base.TryInvokeAction(() => this.sslStream.EndAuthenticateAsServer(asyncResult));
            if (result == false)
            {
                ((ISession)this).Close();
            }
            else
            {
                this.TryBeginRead();
            }
        }

        /// <summary>
        /// 尝试开始接收数据
        /// </summary>
        private void TryBeginRead()
        {
            if (this.IsConnected == false)
            {
                return;
            }

            base.TryInvokeAction(() =>
                this.sslStream.BeginRead(
                this.bufferRange.Buffer,
                this.bufferRange.Offset,
                this.bufferRange.Count,
                this.EndRead,
                null), () => this.DisconnectHandler(this));
        }

        /// <summary>
        /// 接收数据完成后
        /// </summary>
        /// <param name="asyncResult">异步结果</param>
        private void EndRead(IAsyncResult asyncResult)
        {
            var read = base.TryInvokeFunc(() => this.sslStream.EndRead(asyncResult));
            if (read <= 0)
            {
                this.DisconnectHandler(this);
                return;
            }

            lock (this.RecvBuffer.SyncRoot)
            {
                this.RecvBuffer.Seek(0, SeekOrigin.End);
                this.RecvBuffer.Write(this.bufferRange.Buffer, this.bufferRange.Offset, read);
                this.RecvBuffer.Seek(0, SeekOrigin.Begin);
                this.ReceiveHandler(this);
            }

            // 重新进行一次接收
            this.TryBeginRead();
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
                this.sslStream.BeginWrite(
                byteRange.Buffer,
                byteRange.Offset,
                byteRange.Count,
                this.EndWirte,
                null));
        }

        /// <summary>
        /// 发送完成后
        /// </summary>
        /// <param name="asyncResult">异步结果</param>
        private void EndWirte(IAsyncResult asyncResult)
        {
            if (this.IsConnected == false || base.TryInvokeAction(() => this.sslStream.EndWrite(asyncResult)) == false)
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

            if (disposing == true)
            {
                this.targetHost = null;
                this.sslStream = null;
                this.certificate = null;
                this.certificateValidationCallback = null;
                this.bufferRange = null;
            }
        }

        /// <summary>
        /// 异步发送数据
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

            this.sslStream.Write(byteRange.Buffer, byteRange.Offset, byteRange.Count);
        }
    }
}

