using NetworkSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// Http监听服务
    /// </summary>
    public abstract class HttpServerBase : TcpServerBase<SessionBase>
    {
        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <returns></returns>
        protected override SessionBase OnCreateSession()
        {
            return new SessionBase();
        }

        /// <summary>
        /// 收到tcp请求
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="buffer">数据</param>
        protected override void OnReceive(SessionBase session, ReceiveBuffer buffer)
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
            catch (Exception)
            {
                session.Close();
            }
        }

        /// <summary>
        /// 收到Http请求
        /// </summary>       
        /// <param name="request">请求对象</param>
        /// <param name="response">回复对象</param>
        protected abstract void OnHttpRequest(HttpRequest request, HttpResponse response);
    }
}
