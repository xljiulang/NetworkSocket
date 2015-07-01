using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Fast的Api服务抽象类
    /// </summary>
    public abstract class FastApiService : IFastApiService, IAuthorizationFilter, IActionFilter, IExceptionFilter
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
        /// 获取关联的服务器实例
        /// </summary>
        private FastTcpServer Server
        {
            get
            {
                return currentContext.Session.Server;
            }
        }

        /// <summary>
        /// 执行Api行为
        /// </summary>   
        /// <param name="actionContext">上下文</param>      
        void IFastApiService.Execute(ActionContext actionContext)
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
            FastTcpCommon.SetRemoteException(actionContext.Session, exceptionContext);
            this.ExecExceptionFilters(actionfilters, exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exception;
            }
        }

        /// <summary>
        /// 调用自身实现的Api行为
        /// 将返回值发送给客户端        
        /// </summary>       
        /// <param name="actionContext">上下文</param>       
        /// <param name="filters">过滤器</param>
        private void ExecuteAction(ActionContext actionContext, IEnumerable<IFilter> filters)
        {
            var action = actionContext.Action;
            var packet = actionContext.Packet;
            var session = actionContext.Session;
            var serializer = session.Server.Serializer;
            action.ParameterValues = packet.GetBodyParameters(serializer, action.ParameterTypes);

            this.ExecFiltersBeforeAction(filters, actionContext);
            var returnValue = action.Execute(this, action.ParameterValues);
            this.ExecFiltersAfterAction(filters, actionContext);

            // 返回数据
            if (action.IsVoidReturn == false && session.IsConnected)
            {
                packet.Body = serializer.Serialize(returnValue);
                session.Send(packet.ToByteRange());
            }
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
            }
            ((IAuthorizationFilter)this).OnAuthorization(actionContext);
            foreach (var filter in actionFilters)
            {
                var authorizationFilter = filter as IAuthorizationFilter;
                if (authorizationFilter != null)
                {
                    authorizationFilter.OnAuthorization(actionContext);
                }
            }

            // OnExecuting
            foreach (var globalFilter in this.Server.GlobalFilter.ActionFilters)
            {
                globalFilter.OnExecuting(actionContext);
            }

            ((IActionFilter)this).OnExecuting(actionContext);

            foreach (var filter in actionFilters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuting(actionContext);
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
            }

            // 自身过滤器
            ((IActionFilter)this).OnExecuted(actionContext);

            // 特性过滤器
            foreach (var filter in actionFilters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuted(actionContext);
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
                if (exceptionContext.ExceptionHandled == false)
                {
                    filter.OnException(exceptionContext);
                }
            }

            if (exceptionContext.ExceptionHandled == false)
            {
                ((IExceptionFilter)this).OnException(exceptionContext);
            }

            foreach (var filter in actionFilters)
            {
                var exceptionFilter = filter as IExceptionFilter;
                if (exceptionFilter != null && exceptionContext.ExceptionHandled == false)
                {
                    exceptionFilter.OnException(exceptionContext);
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
        ~FastApiService()
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
