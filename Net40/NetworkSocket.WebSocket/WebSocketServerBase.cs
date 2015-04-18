using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示WebSocket服务基础类
    /// 只支持 RFC 6455 协议
    /// </summary>    
    public abstract class WebSocketServerBase : TcpServerBase<Response, FrameRequest>
    {
        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法      
        /// 返回的每一个数据包，将触发一次OnRecvComplete方法       
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="builder">接收到的历史数据</param>
        /// <returns></returns>
        protected override IEnumerable<FrameRequest> OnReceive(IClient<Response> client, ByteBuilder builder)
        {
            var contentBuilder = client.TagData.TryGet<ByteBuilder>("ContentBuilder");
            if (contentBuilder != null)
            {
                return this.GetFrameRequests(builder, contentBuilder);
            }

            // 是握手请求
            if (this.ProcessHandshake(client, builder) == true)
            {
                client.TagData.Set("ContentBuilder", new ByteBuilder());
            }
            return Enumerable.Empty<FrameRequest>();
        }

        /// <summary>
        /// 处理握手
        /// 通过则返回true
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="builder">接收到的数据</param>
        private bool ProcessHandshake(IClient<Response> client, ByteBuilder builder)
        {
            var request = HandshakeRequest.From(builder);
            if (request == null)
            {
                client.Close(StatusCodes.ProtocolError);
                return false;
            }

            var code = StatusCodes.NormalClosure;
            if (this.OnHandshake(client, request, out code) == false)
            {
                client.Close(code);
                return false;
            }

            // 握手成功
            var response = new HandshakeResponse(request);
            client.Send(response);
            return true;
        }


        /// <summary>
        /// 获取请求帧
        /// </summary>
        /// <param name="builder">接收到的数据</param>
        /// <param name="contentBuilder">处理后的内容数据</param>
        /// <returns></returns>
        private IEnumerable<FrameRequest> GetFrameRequests(ByteBuilder builder, ByteBuilder contentBuilder)
        {
            FrameRequest request;
            while ((request = FrameRequest.From(builder, contentBuilder)) != null)
            {
                yield return request;
            }
        }

        /// <summary>
        /// 当收到到数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="tRecv">接收到的数据类型</param>
        protected override void OnRecvComplete(IClient<Response> client, FrameRequest tRecv)
        {
            switch (tRecv.Frame)
            {
                case FrameCodes.Close:
                    var reason = StatusCodes.NormalClosure;
                    if (tRecv.Content.Length > 1)
                    {
                        var status = ByteConverter.ToUInt16(tRecv.Content, 0, Endians.Big);
                        reason = (StatusCodes)status;
                    }
                    this.OnClose(client, reason);
                    client.Close();
                    break;

                case FrameCodes.Binary:
                    this.OnBinary(client, tRecv.Content);
                    break;

                case FrameCodes.Text:
                    var content = Encoding.UTF8.GetString(tRecv.Content);
                    this.OnText(client, content);
                    break;

                case FrameCodes.Ping:
                    client.Send(new FrameResponse(FrameCodes.Pong, tRecv.Content));
                    this.OnPing(client, tRecv.Content);
                    break;

                case FrameCodes.Pong:
                    this.OnPong(client, tRecv.Content);
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
        /// <param name="client">客户端</param>
        /// <param name="request">握手请求</param>
        /// <param name="code">不通过原因</param>
        /// <returns></returns>
        protected virtual bool OnHandshake(IClient<Response> client, HandshakeRequest request, out StatusCodes code)
        {
            code = StatusCodes.NormalClosure;
            return true;
        }

        /// <summary>
        /// 收到文本请求类型时触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">文本内容</param>
        protected virtual void OnText(IClient<Response> client, string content)
        {
        }

        /// <summary>
        /// 收到二进制类型请求时触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnBinary(IClient<Response> client, byte[] content)
        {
        }

        /// <summary>
        /// 收到Ping请求时触发此方法
        /// 在触发此方法之前，基础服务已自动将Pong回复此客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnPing(IClient<Response> client, byte[] content)
        {
        }

        /// <summary>
        /// Ping后客户端将回复Pong触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnPong(IClient<Response> client, byte[] content)
        {
        }

        /// <summary>
        /// 收到客户端关闭信息
        /// 在触发此方法后，基础服务将自动安全回收此客户端对象
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="code">关闭码</param>
        protected virtual void OnClose(IClient<Response> client, StatusCodes code)
        {
        }
    }
}
