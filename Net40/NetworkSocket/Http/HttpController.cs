using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NetworkSocket.Http
{
    /// <summary>
    /// Http控制器基类
    /// </summary>
    public abstract class HttpController : HttpFilterAttribute, IHttpController
    {
        /// <summary>
        /// 线程唯一上下文
        /// </summary>
        [ThreadStatic]
        private static ActionContext currentContext;

        /// <summary>
        /// 获取当前Api行为上下文
        /// </summary>
        protected ActionContext CurrentContext
        {
            get
            {
                return currentContext;
            }
            private set
            {
                currentContext = value;
            }
        }

        /// <summary>
        /// 获取请求上下文对象
        /// </summary>
        public HttpRequest Request
        {
            get
            {
                return this.CurrentContext.Request;
            }
        }

        /// <summary>
        /// 获取回复上下文对象
        /// </summary>
        public HttpResponse Response
        {
            get
            {
                return this.CurrentContext.Response;
            }
        }

        /// <summary>
        /// 获取http服务实例
        /// </summary>
        public HttpServer Server { get; internal set; }

        /// <summary>
        /// 执行Api行为
        /// </summary>   
        /// <param name="actionContext">上下文</param>      
        void IHttpController.Execute(ActionContext actionContext)
        {
            this.CurrentContext = actionContext;
            var filters = this.Server.FilterAttributeProvider.GetActionFilters(actionContext.Action);

            try
            {
                this.ExecuteAction(actionContext, filters);
            }
            catch (AggregateException exception)
            {
                foreach (var inner in exception.InnerExceptions)
                {
                    this.ProcessExecutingException(actionContext, filters, inner);
                }
            }
            catch (Exception exception)
            {
                this.ProcessExecutingException(actionContext, filters, exception);
            }
            finally
            {
                this.CurrentContext = null;
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
            this.ExecExceptionFilters(actionfilters, exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exception;
            }

            var result = exceptionContext.Result == null ? new ErrorResult { Status = 500, Errors = exception.Message } : exceptionContext.Result;
            result.ExecuteResult(actionContext);
        }

        /// <summary>
        /// 调用自身实现的Api行为
        /// 将返回值发送给客户端        
        /// </summary>       
        /// <param name="actionContext">上下文</param>       
        /// <param name="filters">过滤器</param>
        private void ExecuteAction(ActionContext actionContext, IEnumerable<IFilter> filters)
        {
            this.ExecFiltersBeforeAction(filters, actionContext);
            var result = actionContext.Result;

            if (result == null)
            {
                var action = actionContext.Action;
                action.ParameterValues = new object[action.ParameterInfos.Length];

                for (var i = 0; i < action.ParameterValues.Length; i++)
                {
                    var paramter = action.ParameterInfos[i];
                    var pValue = this.Server.ModelBinder.BindModel(actionContext.Request, paramter);
                    action.ParameterValues[i] = pValue;
                }

                result = action.Execute(this, action.ParameterValues) as ActionResult;
                if (result == null) // 直接在方法体里return null
                {
                    throw new Exception("ActionResult不能为null，请使用EmptyResult替代");
                }

                this.ExecFiltersAfterAction(filters, actionContext);
                if (actionContext.Result != null)
                {
                    result = actionContext.Result;
                }
            }

            result.ExecuteResult(actionContext);
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
        /// 生成文件结果
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        protected virtual FileResult File(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var contenType = this.Server.MIMECollection.GetContentType(extension);
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
            var contentDisposition = "attachment; filename=" + HttpUtility.UrlEncode(fileName);
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
            var totalFilters = this.Server
                .GlobalFilters
                .Cast<IFilter>()
                .Concat(new[] { (IFilter)this })
                .Concat(filters);

            foreach (var filter in totalFilters)
            {
                filter.OnExecuting(actionContext);
                if (actionContext.Result != null) return;
            }
        }

        /// <summary>
        /// 在Api行为后执行过滤器
        /// </summary>       
        /// <param name="filters">Api行为过滤器</param>
        /// <param name="actionContext">上下文</param>       
        private void ExecFiltersAfterAction(IEnumerable<IFilter> filters, ActionContext actionContext)
        {
            var totalFilters = this.Server
                  .GlobalFilters
                  .Cast<IFilter>()
                  .Concat(new[] { (IFilter)this })
                  .Concat(filters);

            foreach (var filter in totalFilters)
            {
                filter.OnExecuted(actionContext);
                if (actionContext.Result != null) return;
            }
        }

        /// <summary>
        /// 执行异常过滤器
        /// </summary>       
        /// <param name="filters">Api行为过滤器</param>
        /// <param name="exceptionContext">上下文</param>       
        private void ExecExceptionFilters(IEnumerable<IFilter> filters, ExceptionContext exceptionContext)
        {
            var totalFilters = this.Server
                 .GlobalFilters
                 .Cast<IFilter>()
                 .Concat(new[] { (IFilter)this })
                 .Concat(filters);

            foreach (var filter in totalFilters)
            {
                filter.OnException(exceptionContext);
                if (exceptionContext.ExceptionHandled == true) return;
            }
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
