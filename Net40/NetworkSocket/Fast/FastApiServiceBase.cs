using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkSocket.Fast.Context;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Fast的Api服务抽象类   
    /// </summary>
    public abstract class FastApiServiceBase : IFastApiService, IAuthorizationFilter, IActionFilter, IExceptionFilter
    {
        /// <summary>
        /// 线程唯一上下文
        /// </summary>
        [ThreadStatic]
        private static ServerActionContext currentContext;

        /// <summary>
        /// 获取当前Api行为上下文
        /// </summary>
        protected ServerActionContext CurrentContext
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
        /// 执行Api行为
        /// </summary>   
        /// <param name="actionContext">上下文</param>      
        void IFastApiService.Execute(ServerActionContext actionContext)
        {
            var filters = actionContext.FastTcpServer.FilterAttributeProvider.GetActionFilters(actionContext.Action);

            try
            {
                this.CurrentContext = actionContext;
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
        private void ProcessExecutingException(ServerActionContext actionContext, IEnumerable<IFilter> actionfilters, Exception exception)
        {
            var exceptionContext = new ServerExceptionContext(actionContext, new ActionException(actionContext, exception));
            FastTcpCommon.SetRemoteException(actionContext.FastTcpServer.Serializer, exceptionContext);
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
        private void ExecuteAction(ServerActionContext actionContext, IEnumerable<IFilter> filters)
        {
            // 执行Filter
            this.ExecFiltersBeforeAction(filters, actionContext);

            var parameters = FastTcpCommon.GetApiActionParameters(actionContext.FastTcpServer.Serializer, actionContext);
            var returnValue = actionContext.Action.Execute(this, parameters);

            // 执行Filter
            this.ExecFiltersAfterAction(filters, actionContext);

            // 返回数据
            if (actionContext.Action.IsVoidReturn == false && actionContext.Client.IsConnected)
            {
                var returnBytes = actionContext.FastTcpServer.Serializer.Serialize(returnValue);
                actionContext.Packet.Body = returnBytes;
                actionContext.Client.Send(actionContext.Packet);
            }
        }

        /// <summary>
        /// 调用客户端实现的Api        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param>    
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ContextNullException"></exception>
        /// <exception cref="SocketException"></exception>         
        protected Task InvokeApi(IClient<FastPacket> client, string api, params object[] parameters)
        {
            if (this.CurrentContext == null)
            {
                throw new ContextNullException("CurrentContext上下文对象为空");
            }
            return this.CurrentContext.FastTcpServer.InvokeApi(client, api, parameters);
        }

        /// <summary>
        /// 调用客户端实现的Api   
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="api">api</param>
        /// <param name="parameters">参数</param>     
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ContextNullException"></exception>
        /// <exception cref="SocketException"></exception> 
        /// <exception cref="RemoteException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <returns>远程数据任务</returns>  
        protected Task<T> InvokeApi<T>(IClient<FastPacket> client, string api, params object[] parameters)
        {
            if (this.CurrentContext == null)
            {
                throw new ContextNullException("CurrentContext上下文对象为空");
            }
            return this.CurrentContext.FastTcpServer.InvokeApi<T>(client, api, parameters);
        }


        #region IFilter重写
        /// <summary>
        /// 授权时触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        protected virtual void OnAuthorization(ServerActionContext filterContext)
        {
        }

        /// <summary>
        /// 在执行Api行为前触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        protected virtual void OnExecuting(ServerActionContext filterContext)
        {
        }

        /// <summary>
        /// 在执行Api行为后触发
        /// </summary>
        /// <param name="filterContext">上下文</param>      
        protected virtual void OnExecuted(ServerActionContext filterContext)
        {
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="filterContext">上下文</param>
        protected virtual void OnException(ServerExceptionContext filterContext)
        {
        }
        #endregion

        #region ExecFilters
        /// <summary>
        /// 在Api行为前 执行过滤器
        /// </summary>       
        /// <param name="actionFilters">Api行为过滤器</param>
        /// <param name="actionContext">上下文</param>   
        private void ExecFiltersBeforeAction(IEnumerable<IFilter> actionFilters, ServerActionContext actionContext)
        {
            // OnAuthorization
            foreach (var globalFilter in GlobalFilters.AuthorizationFilters)
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
            foreach (var globalFilter in GlobalFilters.ActionFilters)
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
        private void ExecFiltersAfterAction(IEnumerable<IFilter> actionFilters, ServerActionContext actionContext)
        {
            // 全局过滤器
            foreach (var globalFilter in GlobalFilters.ActionFilters)
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
        private void ExecExceptionFilters(IEnumerable<IFilter> actionFilters, ServerExceptionContext exceptionContext)
        {
            foreach (var filter in GlobalFilters.ExceptionFilters)
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
        void IAuthorizationFilter.OnAuthorization(ServerActionContext filterContext)
        {
            this.OnAuthorization(filterContext);
        }

        /// <summary>
        /// 在执行Api行为前触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        void IActionFilter.OnExecuting(ServerActionContext filterContext)
        {
            this.OnExecuting(filterContext);
        }

        /// <summary>
        /// 在执行Api行为后触发
        /// </summary>
        /// <param name="filterContext">上下文</param>   
        void IActionFilter.OnExecuted(ServerActionContext filterContext)
        {
            this.OnExecuted(filterContext);
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="filterContext">上下文</param>  
        void IExceptionFilter.OnException(ServerExceptionContext filterContext)
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
        ~FastApiServiceBase()
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
