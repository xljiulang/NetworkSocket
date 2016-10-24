using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http中间件抽象类
    /// </summary>
    public abstract class HttpMiddlewareBase : IMiddleware
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
            if (context.Session.IsProtocol("http") == false)
            {
                return this.Next.Invoke(context);
            }
            return this.OnHttpRequest(context);
        }


        /// <summary>
        /// 收到http请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private Task OnHttpRequest(IContenxt context)
        {
            try
            {
                var result = HttpRequestParser.Parse(context);
                if (result.IsHttp == false)
                {
                    return this.Next.Invoke(context);
                }

                if (result.Request == null)
                {
                    return new Task(() => { });
                }

                if (result.Request.IsWebsocketRequest() == true)
                {
                    return this.Next.Invoke(context);
                }

                if (context.Session.Protocol == null)
                {
                    context.Session.SetProtocolWrapper("http", null);
                }

                context.Stream.Clear(result.PackageLength);

                return new Task(() =>
                {
                    var response = new HttpResponse(context.Session);
                    var requestContext = new RequestContext(result.Request, response);
                    this.OnHttpRequest(context, requestContext);
                });
            }
            catch (HttpException ex)
            {
                return new Task(() => this.OnException(context.Session, ex));
            }
            catch (Exception ex)
            {
                return new Task(() => this.OnException(context.Session, new HttpException(500, ex.Message)));
            }
        }

        /// <summary>
        /// 异常时
        /// </summary>
        /// <param name="session">产生异常的会话</param>
        /// <param name="exception">异常</param>
        protected virtual void OnException(ISession session, Exception exception)
        {
            var httpException = exception as HttpException;
            if (httpException == null)
            {
                httpException = new HttpException(500, exception.Message);
            }
            var result = new ErrorResult(httpException);
            var response = new HttpResponse(session);
            result.ExecuteResult(response);
        }

        /// <summary>
        /// 收到Http请求时触发
        /// </summary>       
        /// <param name="context">上下文</param>
        /// <param name="requestContext">请求上下文对象</param>
        protected abstract void OnHttpRequest(IContenxt context, RequestContext requestContext);
    }
}
