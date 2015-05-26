using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// WebSocket服务抽象类
    /// 只支持 RFC 6455 协议
    /// </summary>
    /// <typeparam name="T">会话类型</typeparam>
    public abstract class WebSocketServerBase<T> : TcpServerBase<T> where T : WebSocketSession
    {
        /// <summary>
        /// 当接收到会话对象的数据时，将触发此方法  
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="buffer">接收到的历史数据</param>   
        protected override void OnReceive(T session, ReceiveBuffer buffer)
        {
            var handshaked = session.TagData.TryGet<bool>("HANDSHAKED");
            if (handshaked == false)
            {
                this.ProcessHandshake(session, buffer);
            }
            else
            {
                this.ProcessRequest(session, buffer);
            }
        }

        /// <summary>
        /// 处理握手        
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="buffer">接收到的数据</param>
        private void ProcessHandshake(T session, ReceiveBuffer buffer)
        {
            var request = HttpRequest.From(buffer);
            if (this.OnHandshake(session, request) == false)
            {
                session.Close();
                return;
            }

            // 握手成功          
            try
            {
                var response = new HandshakeResponse(request);
                session.SendResponse(response);
            }
            finally
            {
                session.TagData.Set("HANDSHAKED", true);
            }
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="buffer">接收到的历史数据</param>
        private void ProcessRequest(T session, ReceiveBuffer buffer)
        {
            var requests = this.GetFrameRequestsFromBuffer(buffer);
            foreach (var request in requests)
            {
                // 新线程处理业务内容
                Task.Factory.StartNew(() => this.OnRecvRequest(session, request));
            }
        }


        /// <summary>
        /// 解析请求帧
        /// </summary>
        /// <param name="buffer">接收到的历史数据</param>
        /// <returns></returns>
        private IEnumerable<FrameRequest> GetFrameRequestsFromBuffer(ReceiveBuffer buffer)
        {
            FrameRequest request;
            while ((request = FrameRequest.From(buffer)) != null)
            {
                yield return request;
            }
        }


        /// <summary>
        /// 收到到数据帧请求
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="request">数据帧</param>
        private void OnRecvRequest(T session, FrameRequest request)
        {
            switch (request.Frame)
            {
                case FrameCodes.Close:
                    var reason = StatusCodes.NormalClosure;
                    if (request.Content.Length > 1)
                    {
                        var status = ByteConverter.ToUInt16(request.Content, 0, Endians.Big);
                        reason = (StatusCodes)status;
                    }
                    this.OnClose(session, reason);
                    session.Close();
                    break;

                case FrameCodes.Binary:
                    this.OnBinary(session, request.Content);
                    break;

                case FrameCodes.Text:
                    var content = Encoding.UTF8.GetString(request.Content);
                    this.OnText(session, content);
                    break;

                case FrameCodes.Ping:
                    try
                    {
                        session.SendResponse(new FrameResponse(FrameCodes.Pong, request.Content));
                    }
                    finally
                    {
                        this.OnPing(session, request.Content);
                    }
                    break;

                case FrameCodes.Pong:
                    this.OnPong(session, request.Content);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 当收到握手请求时，将触发此方法
        /// 返回true说明握手通过
        /// 否则基础服务将自动安全关闭客户端对象
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="request">握手请求</param>     
        /// <returns></returns>
        protected virtual bool OnHandshake(T session, HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.IsWebsocketRequest();
        }

        /// <summary>
        /// 收到文本请求类型时触发此方法
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="content">文本内容</param>
        protected virtual void OnText(T session, string content)
        {
        }

        /// <summary>
        /// 收到二进制类型请求时触发此方法
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnBinary(T session, byte[] content)
        {
        }

        /// <summary>
        /// 收到Ping请求时触发此方法
        /// 在触发此方法之前，基础服务已自动将Pong回复此会话
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnPing(T session, byte[] content)
        {
        }

        /// <summary>
        /// Ping后会话对象将回复Pong触发此方法
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnPong(T session, byte[] content)
        {
        }

        /// <summary>
        /// 收到会话的关闭信息
        /// 在触发此方法后，基础服务将自动安全回收此会话对象
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="code">关闭码</param>
        protected virtual void OnClose(T session, StatusCodes code)
        {
        }
    }
}
