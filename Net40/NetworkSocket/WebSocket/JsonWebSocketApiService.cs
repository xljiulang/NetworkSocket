using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示JsonWebsocket协议的Api服务基类
    /// </summary>
    public abstract class JsonWebSocketApiService : JsonWebSocketFilterAttribute, IJsonWebSocketApiService
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
        private JsonWebSocketServer Server
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
        void IJsonWebSocketApiService.Execute(ActionContext actionContext)
        {
            var filters = Enumerable.Empty<IFilter>();
            try
            {
                this.CurrentContext = actionContext;
                filters = this.Server.FilterAttributeProvider.GetActionFilters(actionContext.Action);
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
            this.Server.SendRemoteException(exceptionContext);
            this.ExecAllExceptionFilters(actionfilters, exceptionContext);
        }

        /// <summary>
        /// 调用自身实现的Api行为
        /// 将返回值发送给客户端        
        /// </summary>       
        /// <param name="actionContext">上下文</param>       
        /// <param name="filters">过滤器</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>当正常执行输出Api的结果时返回true</returns>
        private bool ExecuteAction(ActionContext actionContext, IEnumerable<IFilter> filters)
        {
            var packet = actionContext.Packet;
            var action = actionContext.Action;
            var session = actionContext.Session;
            var serializer = this.Server.JsonSerializer;
            var parameters = this.GetAndUpdateParameterValues(actionContext);

            // Api执行前
            this.ExecFiltersBeforeAction(filters, actionContext);
            if (actionContext.Result != null)
            {
                var exceptionContext = new ExceptionContext(actionContext, actionContext.Result);
                this.Server.SendRemoteException(exceptionContext);
                return false;
            }

            // 执行Api            
            var apiResult = action.Execute(this, parameters);

            // Api执行后
            this.ExecFiltersAfterAction(filters, actionContext);
            if (actionContext.Result != null)
            {
                var exceptionContext = new ExceptionContext(actionContext, actionContext.Result);
                this.Server.SendRemoteException(exceptionContext);
                return false;
            }

            // 返回数据
            if (action.IsVoidReturn == false && session.IsConnected)
            {
                packet.body = apiResult;
                var packetJson = serializer.Serialize(packet);
                session.SendText(packetJson);
            }
            return true;
        }

        /// <summary>
        /// 获取和更新Api行为的参数值
        /// </summary> 
        /// <param name="context">上下文</param>        
        /// <exception cref="ArgumentException"></exception>    
        /// <returns></returns>
        private object[] GetAndUpdateParameterValues(ActionContext context)
        {
            var body = context.Packet.body as IList;
            if (body == null)
            {
                throw new ArgumentException("body参数必须为数组");
            }

            if (body.Count != context.Action.ParameterTypes.Length)
            {
                throw new ArgumentException("body参数数量不正确");
            }

            var parameters = new object[body.Count];
            var serializer = context.Session.Server.JsonSerializer;

            for (var i = 0; i < body.Count; i++)
            {
                var bodyParameter = body[i];
                var parameterType = context.Action.ParameterTypes[i];
                parameters[i] = serializer.Convert(bodyParameter, parameterType);
            }
            context.Action.ParameterValues = parameters;
            return parameters;
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

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exceptionContext.Exception;
            }
        }

        /// <summary>
        /// 获取全部的过滤器
        /// </summary>
        /// <param name="filters">行为过滤器</param>
        /// <returns></returns>
        private IEnumerable<IFilter> GetTotalFilters(IEnumerable<IFilter> filters)
        {
            return this.Server
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
        ~JsonWebSocketApiService()
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
