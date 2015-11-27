using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkSocket.Core;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http控制器
    /// </summary>
    public class HttpController : IHttpController, IAuthorizationFilter, IActionFilter, IExceptionFilter
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
            var exceptionContext = new ExceptionContext(actionContext, new ApiExecuteException(actionContext, exception));
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


        #region IFilter重写
        /// <summary>
        /// 授权时触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        protected virtual void OnAuthorization(ActionContext filterContext)
        {
        }

        /// <summary>
        /// 在执行Api行为前触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        protected virtual void OnExecuting(ActionContext filterContext)
        {
        }

        /// <summary>
        /// 在执行Api行为后触发
        /// </summary>
        /// <param name="filterContext">上下文</param>      
        protected virtual void OnExecuted(ActionContext filterContext)
        {
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="filterContext">上下文</param>
        protected virtual void OnException(ExceptionContext filterContext)
        {
        }
        #endregion

        #region ExecFilters
        /// <summary>
        /// 在Api行为前 执行过滤器
        /// </summary>       
        /// <param name="actionFilters">Api行为过滤器</param>
        /// <param name="actionContext">上下文</param>   
        private void ExecFiltersBeforeAction(IEnumerable<IFilter> actionFilters, ActionContext actionContext)
        {
            // OnAuthorization
            foreach (var globalFilter in this.Server.GlobalFilter.AuthorizationFilters)
            {
                globalFilter.OnAuthorization(actionContext);
                if (actionContext.Result != null) return;
            }

            ((IAuthorizationFilter)this).OnAuthorization(actionContext);
            if (actionContext.Result != null) return;

            foreach (var filter in actionFilters)
            {
                var authorizationFilter = filter as IAuthorizationFilter;
                if (authorizationFilter != null)
                {
                    authorizationFilter.OnAuthorization(actionContext);
                    if (actionContext.Result != null) return;
                }
            }

            // OnExecuting
            foreach (var globalFilter in this.Server.GlobalFilter.ActionFilters)
            {
                globalFilter.OnExecuting(actionContext);
                if (actionContext.Result != null) return;
            }

            ((IActionFilter)this).OnExecuting(actionContext);
            if (actionContext.Result != null) return;

            foreach (var filter in actionFilters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuting(actionContext);
                    if (actionContext.Result != null) return;
                }
            }
        }

        /// <summary>
        /// 在Api行为后执行过滤器
        /// </summary>       
        /// <param name="actionFilters">Api行为过滤器</param>
        /// <param name="actionContext">上下文</param>       
        private void ExecFiltersAfterAction(IEnumerable<IFilter> actionFilters, ActionContext actionContext)
        {
            // 全局过滤器
            foreach (var globalFilter in this.Server.GlobalFilter.ActionFilters)
            {
                globalFilter.OnExecuted(actionContext);
                if (actionContext.Result != null) return;
            }

            // 自身过滤器
            ((IActionFilter)this).OnExecuted(actionContext);
            if (actionContext.Result != null) return;

            // 特性过滤器
            foreach (var filter in actionFilters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuted(actionContext);
                    if (actionContext.Result != null) return;
                }
            }
        }

        /// <summary>
        /// 执行异常过滤器
        /// </summary>       
        /// <param name="actionFilters">Api行为过滤器</param>
        /// <param name="exceptionContext">上下文</param>       
        private void ExecExceptionFilters(IEnumerable<IFilter> actionFilters, ExceptionContext exceptionContext)
        {
            foreach (var filter in this.Server.GlobalFilter.ExceptionFilters)
            {
                filter.OnException(exceptionContext);
                if (exceptionContext.ExceptionHandled) return;
            }

            ((IExceptionFilter)this).OnException(exceptionContext);
            if (exceptionContext.ExceptionHandled) return;

            foreach (var filter in actionFilters)
            {
                var exceptionFilter = filter as IExceptionFilter;
                if (exceptionFilter != null)
                {
                    exceptionFilter.OnException(exceptionContext);
                    if (exceptionContext.ExceptionHandled) return;
                }
            }
        }
        #endregion

        #region IFilter
        /// <summary>
        /// 获取或设置排序
        /// </summary>
        int IFilter.Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 是否允许多个实例
        /// </summary>
        bool IFilter.AllowMultiple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 授权时触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        void IAuthorizationFilter.OnAuthorization(ActionContext filterContext)
        {
            this.OnAuthorization(filterContext);
        }

        /// <summary>
        /// 在执行Api行为前触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        void IActionFilter.OnExecuting(ActionContext filterContext)
        {
            this.OnExecuting(filterContext);
        }

        /// <summary>
        /// 在执行Api行为后触发
        /// </summary>
        /// <param name="filterContext">上下文</param>   
        void IActionFilter.OnExecuted(ActionContext filterContext)
        {
            this.OnExecuted(filterContext);
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="filterContext">上下文</param>  
        void IExceptionFilter.OnException(ExceptionContext filterContext)
        {
            this.OnException(filterContext);
        }
        #endregion

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
