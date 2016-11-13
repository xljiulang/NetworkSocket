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
        /// 不为null时表示服务器会话对象
        /// </summary>
        private X509Certificate certificate;

        /// <summary>
        /// 远程证书验证回调
        /// </summary>
        private RemoteCertificateValidationCallback certificateValidationCallback;

        /// <summary>
        /// 缓冲区范围
        /// </summary>
        private ArraySegment<byte> bufferRange = BufferManager.GetBuffer();

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
                try
                {
                    this.sslStream.BeginAuthenticateAsServer(
                        this.certificate,
                        this.EndAuthenticateAsServer,
                        null);
                }
                catch (Exception)
                {
                    ((ISession)this).Close();
                }
            }
        }

        /// <summary>
        /// 服务器验证完成后
        /// </summary>
        /// <param name="asyncResult">异步结果</param>
        private void EndAuthenticateAsServer(IAsyncResult asyncResult)
        {
            try
            {
                this.sslStream.EndAuthenticateAsServer(asyncResult);
                this.TryBeginRead();
            }
            catch (Exception)
            {
                ((ISession)this).Close();
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

            try
            {
                this.sslStream.BeginRead(
                    this.bufferRange.Array,
                    this.bufferRange.Offset,
                    this.bufferRange.Count,
                    this.EndRead,
                    null);
            }
            catch (Exception)
            {
                this.DisconnectHandler(this);
            }
        }

        /// <summary>
        /// 接收数据完成后
        /// </summary>
        /// <param name="asyncResult">异步结果</param>
        private async void EndRead(IAsyncResult asyncResult)
        {
            var read = this.ReadSessionStream(asyncResult);
            if (read <= 0)
            {
                this.DisconnectHandler(this);
            }
            else
            {
                lock (this.StreamReader.SyncRoot)
                {
                    this.StreamReader.Stream.Seek(0, SeekOrigin.End);
                    this.StreamReader.Stream.Write(this.bufferRange.Array, this.bufferRange.Offset, read);
                    this.StreamReader.Stream.Seek(0, SeekOrigin.Begin);
                }

                // 重新进行一次接收
                await this.ReceiveAsyncHandler(this);
                this.TryBeginRead();
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        private int ReadSessionStream(IAsyncResult asyncResult)
        {
            try
            {
                return this.sslStream.EndRead(asyncResult);
            }
            catch (Exception)
            {
                return 0;
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
            }
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>  
        /// <exception cref="ArgumentNullException"></exception>        
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public override int Send(ArraySegment<byte> byteRange)
        {
            if (byteRange == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            this.sslStream.Write(byteRange.Array, byteRange.Offset, byteRange.Count);
            return byteRange.Count;
        }

        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <returns></returns>
        public override int Send(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (this.IsConnected == false)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            this.sslStream.Write(buffer);
            return buffer.Length;
        }
    }
}

