using NetworkSocket.Http;
using NetworkSocket.Util;
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
            var isWebSocket = context.Session.IsProtocol("websocket");
            if (isWebSocket == true)
            {
                return this.OnWebSocketFrameRequest(context);
            }

            if (isWebSocket == null)
            {
                return this.OnWebSocketHandshakeRequest(context);
            }

            if (context.Session.IsProtocol("http") == true)
            {
                return this.OnWebSocketHandshake(context);
            }

            return this.Next.Invoke(context);
        }

        /// <summary>
        /// 回复握手，请求体已被http中间件解析
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private Task OnWebSocketHandshake(IContenxt context)
        {
            const string seckey = "Sec-WebSocket-Key";
            var secValue = context.Session.Tag.TryGet<string>(seckey);
            if (string.IsNullOrEmpty(secValue) == true)
            {
                return this.Next.Invoke(context);
            }

            context.Session.Tag.Remove(seckey);
            return this.ResponseHandshake(context, secValue);
        }

        /// <summary>
        /// 收到WebSocket的握手请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private Task OnWebSocketHandshakeRequest(IContenxt context)
        {
            try
            {
                var httpRequest = default(HttpRequest);
                if (HttpRequest.Parse(context, out httpRequest) == false)
                {
                    return this.Next.Invoke(context);
                }

                if (httpRequest == null || httpRequest.IsWebsocketRequest() == false)
                {
                    return this.Next.Invoke(context);
                }

                const string seckey = "Sec-WebSocket-Key";
                var secValue = httpRequest.Headers[seckey];
                return this.ResponseHandshake(context, secValue);
            }
            catch (Exception)
            {
                return this.Next.Invoke(context);
            }
        }

        /// <summary>
        /// 回复握手请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="secValue">Sec-WebSocket-Key</param>
        /// <returns></returns>
        private Task ResponseHandshake(IContenxt context, string secValue)
        {
            return new Task(() =>
            {
                try
                {
                    var wrapper = new WebSocketSession(context.Session);
                    var hansshakeResponse = new HandshakeResponse(secValue);

                    wrapper.SendAsync(hansshakeResponse);
                    this.OnSetProtocolWrapper(context.Session, wrapper);
                }
                catch (Exception) { }
            });
        }

        /// <summary>
        /// 设置会话的包装对象
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="wrapper">包装对象</param>
        protected virtual void OnSetProtocolWrapper(ISession session, WebSocketSession wrapper)
        {
            session.SetProtocolWrapper("websocket", wrapper);
        }

        /// <summary>
        /// 收到WebSocket请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private Task OnWebSocketFrameRequest(IContenxt context)
        {
            var requests = this.GenerateWebSocketRequest(context);
            return new Task(() =>
            {
                foreach (var request in requests)
                {
                    this.OnWebSocketRequest(context, request);
                }
            });
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
                    var request = FrameRequest.Parse(context.Stream);
                    if (request == null)
                    {
                        return list;
                    }
                    list.Add(request);
                }
                catch (NotSupportedException)
                {
                    context.Session.Close();
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
                    var reason = StatusCodes.NormalClosure;
                    if (frameRequest.Content.Length > 1)
                    {
                        var status = ByteConverter.ToUInt16(frameRequest.Content, 0, Endians.Big);
                        reason = (StatusCodes)status;
                    }
                    this.OnClose(context, reason);
                    context.Session.Close();
                    break;

                case FrameCodes.Binary:
                    this.OnBinary(context, frameRequest.Content);
                    break;

                case FrameCodes.Text:
                    var content = Encoding.UTF8.GetString(frameRequest.Content);
                    this.OnText(context, content);
                    break;

                case FrameCodes.Ping:
                    try
                    {
                        var session = (WebSocketSession)context.Session.Wrapper;
                        session.SendAsync(new FrameResponse(FrameCodes.Pong, frameRequest.Content));
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
        /// <param name="content">文本内容</param>
        protected virtual void OnText(IContenxt context, string content)
        {
        }

        /// <summary>
        /// 收到二进制类型请求时触发此方法
        /// </summary>
        /// <param name="context">会话对象</param>
        /// <param name="content">二进制内容</param>
        protected virtual void OnBinary(IContenxt context, byte[] content)
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
        protected virtual void OnClose(IContenxt context, StatusCodes code)
        {
        }
    }
}
