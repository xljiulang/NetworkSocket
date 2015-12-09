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
    public abstract class HttpServerBase : TcpServerBase<SessionBase>
    {
        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <returns></returns>
        protected sealed override SessionBase OnCreateSession()
        {
            return new SessionBase();
        }

        /// <summary>
        /// 收到tcp请求
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="buffer">数据</param>
        protected sealed override void OnReceive(SessionBase session, ReceiveStream buffer)
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
                var response = new HttpResponse(session);
                response.Status = ex.Status;
                response.Write(ex.Message);
            }
            catch (Exception ex)
            {
                var response = new HttpResponse(session);
                response.Status = 500;
                response.Write(ex.Message);
                response.End();
            }
        }

        /// <summary>
        /// 收到Http请求时触发
        /// </summary>       
        /// <param name="request">请求对象</param>
        /// <param name="response">回复对象</param>
        protected abstract void OnHttpRequest(HttpRequest request, HttpResponse response);
    }
}
