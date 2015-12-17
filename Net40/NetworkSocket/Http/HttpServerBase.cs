using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http监听服务抽象类
    /// </summary>
    public abstract class HttpServerBase : TcpServerBase<HttpSession>
    {
        /// <summary>
        /// 获取所有可http事件推送会话
        /// </summary>
        public IEnumerable<HttpSession> EventSessions
        {
            get
            {
                return this.AllSessions.Where(item => item.IsEventStream);
            }
        }

        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <returns></returns>
        protected sealed override HttpSession OnCreateSession()
        {
            return new HttpSession();
        }

        /// <summary>
        /// 收到tcp请求
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="buffer">数据</param>
        protected sealed override void OnReceive(HttpSession session, ReceiveStream buffer)
        {
            try
            {
                var request = HttpRequest.Parse(buffer, base.LocalEndPoint, session.RemoteEndPoint);
                if (request != null)
                {
                    this.OnHttpRequest(request, new HttpResponse(session));
                }
            }
            catch (HttpException ex)
            {
                this.OnException(session, ex);
            }
            catch (Exception ex)
            {
                this.OnException(session, new HttpException(500, ex.Message));
            }
        }

        /// <summary>
        /// 异常时
        /// </summary>
        /// <param name="session">产生异常的会话</param>
        /// <param name="exception">异常</param>
        protected override void OnException(HttpSession session, Exception exception)
        {
            if (session == null)
            {
                return;
            }
            var httpException = exception as HttpException;
            var response = new HttpResponse(session);
            response.Status = httpException == null ? 500 : httpException.Status;
            response.Write(exception.Message);
        }

        /// <summary>
        /// 收到Http请求时触发
        /// </summary>       
        /// <param name="request">请求对象</param>
        /// <param name="response">回复对象</param>
        protected abstract void OnHttpRequest(HttpRequest request, HttpResponse response);
    }
}
