using NetworkSocket.Http;
using NetworkSocket.Tasks;
using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示WebSocket中间件抽象类
    /// 只支持 RFC 6455 协议
    /// </summary>
    public abstract class WebSocketMiddlewareBase : IMiddleware
    {
        /// <summary>
        /// 下一个中间件
        /// </summary>
        public IMiddleware Next { get; set; }

        /// <summary>
        /// 执行中间件
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        Task IMiddleware.Invoke(IContenxt context)
        {
            var protocol = context.Session.Protocol;
            if (protocol == Protocol.WebSocket)
            {
                return this.OnWebSocketFrameRequestAsync(context);
            }

            if (protocol == Protocol.None || protocol == Protocol.Http)
            {
                return this.OnWebSocketHandshakeRequestAsync(context);
            }
            else
            {
                return this.Next.Invoke(context);
            }
        }

        /// <summary>
        /// 收到WebSocket的握手请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private async Task OnWebSocketHandshakeRequestAsync(IContenxt context)
        {
            try
            {
                var result = HttpRequestParser.Parse(context);
                if (result.IsHttp == false)
                {
                    await this.Next.Invoke(context);
                    return;
                }

                // 数据未完整
                if (result.Request == null)
                {
                    return;
                }

                if (result.Request.IsWebsocketRequest() == false)
                {
                    await this.Next.Invoke(context);
                    return;
                }

                context.StreamReader.Clear(result.PackageLength);
                const string seckey = "Sec-WebSocket-Key";
                var secValue = result.Request.Headers[seckey];
                this.ResponseHandshake(context, secValue);
            }
            catch (Exception)
            {
                context.StreamReader.Clear();
                context.Session.Close();
            }
        }

        /// <summary>
        /// 回复握手请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="secValue">Sec-WebSocket-Key</param>
        private void ResponseHandshake(IContenxt context, string secValue)
        {
            var wrapper = new WebSocketSession(context.Session);
            var hansshakeResponse = new HandshakeResponse(secValue);

            if (wrapper.TrySend(hansshakeResponse) == true)
            {
                this.OnSetProtocolWrapper(context.Session, wrapper);
            }
        }

        /// <summary>
        /// 设置会话的包装对象
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="wrapper">包装对象</param>
        protected virtual void OnSetProtocolWrapper(ISession session, WebSocketSession wrapper)
        {
            session.SetProtocolWrapper(Protocol.WebSocket, wrapper);
        }

        /// <summary>
        /// 收到WebSocket请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private Task OnWebSocketFrameRequestAsync(IContenxt context)
        {
            var requests = this.GenerateWebSocketRequest(context);
            foreach (var request in requests)
            {
                this.OnWebSocketRequest(context, request);
            }
            return TaskExtend.CompletedTask;
        }

        /// <summary>
        /// 解析生成请求帧
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private IList<FrameRequest> GenerateWebSocketRequest(IContenxt context)
        {
            var list = new List<FrameRequest>();
            while (true)
            {
                try
                {
                    var request = FrameRequest.Parse(context.StreamReader);
                    if (request == null)
                    {
                        return list;
                    }
                    list.Add(request);
                }
                catch (NotSupportedException ex)
                {
                    var session = new WebSocketSession(context.Session);
                    session.Close(StatusCodes.ProtocolError, ex.Message);
                    return list;
                }
            }
        }

        /// <summary>
        /// 收到到数据帧请求
        /// </summary>
        /// <param name="context">会话对象</param>
        /// <param name="frameRequest">数据帧</param>
        private void OnWebSocketRequest(IContenxt context, FrameRequest frameRequest)
        {
            switch (frameRequest.Frame)
            {
                case FrameCodes.Close:
                    var frame = new CloseRequest(frameRequest);
                    this.OnClose(context, frame.StatusCode, frame.CloseReason);
                    context.Session.Close();
                    break;

                case FrameCodes.Binary:
                    this.OnBinary(context, frameRequest);
                    break;

                case FrameCodes.Text: 
                    this.OnText(context, frameRequest);
                    break;

                case FrameCodes.Ping:
                    try
                    {
                        var pong = new FrameResponse(FrameCodes.Pong, frameRequest.Content);
                        var pongContent = pong.ToArraySegment(mask: false);
                        context.Session.Send(pongContent);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        this.OnPing(context, frameRequest.Content);
                    }
                    break;

                case FrameCodes.Pong:
                    context.Session.Publish("Pong", frameRequest.Content);
                    this.OnPong(context, frameRequest.Content);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 收到文本请求类型时触发此方法
        /// </summary>
        /// <param name="context">会话对象</param>
        /// <param name="frame">帧</param>
        protected virtual void OnText(IContenxt context, FrameRequest frame)
        {
        }

        /// <summary>
        /// 收到二进制类型请求时触发此方法
        /// </summary>
        /// <param name="context">会话对象</param>
        /// <param name="frame">帧</param>
        protected virtual void OnBinary(IContenxt context, FrameRequest frame)
        {
        }

        /// <summary>
        /// 收到Ping请求时触发此方法
        /// 在触发此方法之前，基础服务已自动将Pong回复此会话
        /// </summary>
        /// <param name="context">会话对象</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnPing(IContenxt context, byte[] content)
        {
        }

        /// <summary>
        /// Ping后会话对象将回复Pong触发此方法
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnPong(IContenxt context, byte[] content)
        {
        }

        /// <summary>
        /// 收到会话的关闭信息
        /// 在触发此方法后，基础服务将自动安全回收此会话对象
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="code">关闭码</param>
        /// <param name="reason">备注原因</param>
        protected virtual void OnClose(IContenxt context, StatusCodes code, string reason)
        {
        }
    }
}
