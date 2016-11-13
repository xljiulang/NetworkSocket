using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// Http控制器基类
    /// </summary>
    public abstract class HttpController : HttpFilterAttribute, IHttpController
    {
        /// <summary>
        /// 获取http中间件的实例
        /// </summary>
        internal protected HttpMiddleware Middleware { get; internal set; }

        /// <summary>
        /// 获取当前Api行为上下文
        /// </summary>
        protected ActionContext CurrentContext { get; private set; }


        /// <summary>
        /// 获取请求上下文对象
        /// </summary>
        protected HttpRequest Request
        {
            get
            {
                return this.CurrentContext.Request;
            }
        }

        /// <summary>
        /// 获取回复上下文对象
        /// </summary>
        protected HttpResponse Response
        {
            get
            {
                return this.CurrentContext.Response;
            }
        }

        /// <summary>
        /// 异步执行Api行为
        /// </summary>   
        /// <param name="actionContext">上下文</param>      
        async Task IHttpController.ExecuteAsync(ActionContext actionContext)
        {
            var filters = Enumerable.Empty<IFilter>();
            try
            {
                this.CurrentContext = actionContext;
                filters = this.Middleware.FilterAttributeProvider.GetActionFilters(actionContext.Action);
                await this.ExecuteActionAsync(actionContext, filters);
            }
            catch (Exception ex)
            {
                this.ProcessExecutingException(actionContext, filters, ex);
            }
        }

        /// <summary>
        /// 处理Api行为执行过程中产生的异常
        /// </summary>
        /// <param name="actionContext">上下文</param>
        /// <param name="actionfilters">过滤器</param>
        /// <param name="exception">异常项</param>
        private void ProcessExecutingException(ActionContext actionContext, IEnumerable<IFilter> actionfilters, Exception exception)
        {
            var exceptionContext = new ExceptionContext(actionContext, new ApiExecuteException(exception));
            this.ExecAllExceptionFilters(actionfilters, exceptionContext);

            var result = exceptionContext.Result;
            if (result == null)
            {
                result = new ErrorResult { Status = 500, Errors = exception.Message };
            }
            result.ExecuteResult(actionContext);
        }

        /// <summary>
        /// 调用自身实现的Api行为
        /// 将返回值发送给客户端        
        /// </summary>       
        /// <param name="actionContext">上下文</param>       
        /// <param name="filters">过滤器</param>
        /// <returns>如果输出Api的返回结果就返回true</returns>
        private async Task ExecuteActionAsync(ActionContext actionContext, IEnumerable<IFilter> filters)
        {
            this.Middleware.ModelBinder.BindAllParameterValue(actionContext);
            this.ExecFiltersBeforeAction(filters, actionContext);

            if (actionContext.Result != null)
            {
                actionContext.Result.ExecuteResult(actionContext);
            }
            else
            {
                await this.ExecutingActionAsync(actionContext, filters);
            }
        }

        /// <summary>
        /// 异步执行Api
        /// </summary>
        /// <param name="actionContext">上下文</param>
        /// <param name="filters">过滤器</param>
        /// <returns></returns>
        private async Task ExecutingActionAsync(ActionContext actionContext, IEnumerable<IFilter> filters)
        {
            var parameters = actionContext.Action.Parameters.Select(p => p.Value).ToArray();
            var result = await actionContext.Action.ExecuteAsync(this, parameters);

            this.ExecFiltersAfterAction(filters, actionContext);
            if (actionContext.Result != null)
            {
                actionContext.Result.ExecuteResult(actionContext);
                return;
            }

            var actionResult = result as ActionResult;
            if (actionResult == null)
            {
                this.Restful(result).ExecuteResult(actionContext);
            }
            else
            {
                actionResult.ExecuteResult(actionContext);
            }
        }

        /// <summary>
        /// 生成Content结果
        /// </summary>
        /// <param name="content">内容</param>
        /// <returns></returns>
        protected virtual ContentResult Content(string content)
        {
            return new ContentResult(content);
        }

        /// <summary>
        /// 生成json结果
        /// </summary>
        /// <param name="data">内容</param>
        protected virtual JsonResult Json(object data)
        {
            return new JsonResult(data);
        }

        /// <summary>
        /// 生成Restful结果
        /// 直接返回数据类型的Api将使用此结果输出
        /// </summary>
        /// <param name="data">内容</param>
        /// <returns></returns>
        protected virtual RestfulResult Restful(object data)
        {
            return new RestfulResult(data);
        }

        /// <summary>
        /// 生成文件结果
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        protected virtual FileResult File(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var contenType = this.Middleware.MIMECollection[extension];
            return this.File(fileName, contenType);
        }

        /// <summary>
        /// 生成文件结果
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="contenType">内容类型</param>
        /// <returns></returns>
        protected virtual FileResult File(string fileName, string contenType)
        {
            var contentDisposition = "attachment; filename=" + HttpUtility.UrlEncode(fileName, this.Response.Charset);
            return this.File(fileName, contenType, contentDisposition);
        }

        /// <summary>
        /// 生成文件结果
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="contenType">内容类型</param>
        /// <param name="contentDisposition">内容描述</param>
        /// <returns></returns>
        protected virtual FileResult File(string fileName, string contenType, string contentDisposition)
        {
            return new FileResult
            {
                FileName = fileName,
                ContentType = contenType,
                ContentDisposition = contentDisposition
            };
        }

        /// <summary>
        /// 在Api行为前 执行过滤器
        /// </summary>       
        /// <param name="filters">Api行为过滤器</param>
        /// <param name="actionContext">上下文</param>   
        private void ExecFiltersBeforeAction(IEnumerable<IFilter> filters, ActionContext actionContext)
        {
            var totalFilters = this.GetTotalFilters(filters);
            foreach (var filter in totalFilters)
            {
                filter.OnExecuting(actionContext);
                if (actionContext.Result != null) break;
            }
        }

        /// <summary>
        /// 在Api行为后执行过滤器
        /// </summary>       
        /// <param name="filters">Api行为过滤器</param>
        /// <param name="actionContext">上下文</param>       
        private void ExecFiltersAfterAction(IEnumerable<IFilter> filters, ActionContext actionContext)
        {
            var totalFilters = this.GetTotalFilters(filters);
            foreach (var filter in totalFilters)
            {
                filter.OnExecuted(actionContext);
                if (actionContext.Result != null) break;
            }
        }

        /// <summary>
        /// 执行异常过滤器
        /// </summary>       
        /// <param name="filters">Api行为过滤器</param>
        /// <param name="exceptionContext">上下文</param>       
        private void ExecAllExceptionFilters(IEnumerable<IFilter> filters, ExceptionContext exceptionContext)
        {
            var totalFilters = this.GetTotalFilters(filters);
            foreach (var filter in totalFilters)
            {
                filter.OnException(exceptionContext);
                if (exceptionContext.ExceptionHandled == true) break;
            }
        }

        /// <summary>
        /// 获取全部的过滤器
        /// </summary>
        /// <param name="filters">行为过滤器</param>
        /// <returns></returns>
        private IEnumerable<IFilter> GetTotalFilters(IEnumerable<IFilter> filters)
        {
            return this.Middleware
                .GlobalFilters
                .Cast<IFilter>()
                .Concat(new[] { this })
                .Concat(filters);
        }

        #region IDisponse
        /// <summary>
        /// 获取对象是否已释放
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 关闭和释放所有相关资源
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed == false)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
            this.IsDisposed = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~HttpController()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
        }
        #endregion
    }
}
