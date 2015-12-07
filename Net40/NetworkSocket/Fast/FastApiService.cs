using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Fast协议的Api服务基类
    /// </summary>
    public abstract class FastApiService : FastFilterAttribute, IFastApiService
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
            var exceptionContext = new ExceptionContext(actionContext, new ApiExecuteException(exception));
            Common.SetRemoteException(actionContext.Session, exceptionContext);
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
