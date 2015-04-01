using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Fast服务抽象类
    /// 所有自身实现的服务方法的第一个参数是ActionContext类型
    /// </summary>
    public abstract class FastServiceBase : IAuthorizationFilter, IActionFilter, IExceptionFilter, IDisposable
    {
        /// <summary>
        /// 获取或设置序列化工具
        /// </summary>
        internal ISerializer Serializer;

        /// <summary>
        /// 获取过滤委托
        /// </summary>
        internal Func<FastAction, IEnumerable<IFilter>> GetFilters;

        /// <summary>
        /// 线程唯一上下文
        /// </summary>
        [ThreadStatic]
        private static ActionContext staticContext;

        /// <summary>
        /// 获取当前服务行为上下文
        /// </summary>
        public ActionContext CurrentContext
        {
            get
            {
                return staticContext;
            }
            private set
            {
                staticContext = value;
            }
        }

        /// <summary>
        /// 处理服务方法
        /// </summary>
        /// <param name="actionContext">上下文</param>      
        internal void ProcessAction(ActionContext actionContext)
        {
            // 如果是Cmd值对应是Self类型方法 也就是客户端主动调用服务行为
            if (actionContext.Action.Implement == Implements.Self)
            {
                this.TryInvokeAction(actionContext);
                return;
            }

            // 如果是收到返回值 从回调表找出相关回调来调用
            var callBack = CallbackTable.Take(actionContext.Packet.HashCode);
            if (callBack != null)
            {
                var returnBytes = actionContext.Packet.GetBodyParameter().FirstOrDefault();
                callBack(actionContext.Packet.IsException, returnBytes);
            }
        }

        /// <summary>
        /// 调用自身方法
        /// 将返回值发送给客户端
        /// 或将异常发送给客户端
        /// </summary>       
        /// <param name="actionContext">上下文</param>       
        private void TryInvokeAction(ActionContext actionContext)
        {
            // 获取服务行为的特性过滤器
            var filters = this.GetFilters(actionContext.Action);

            try
            {
                // 设置上下文
                this.CurrentContext = actionContext;
                this.InvokeFiltersBefore(filters, actionContext);

                var parameters = FastTcpCommon.GetActionParameters(actionContext, this.Serializer);
                var returnValue = actionContext.Action.Execute(this, parameters);

                // 执行Filter
                this.InvokeFiltersAfter(filters, actionContext);

                if (actionContext.Action.IsVoidReturn == false && actionContext.Client.IsConnected)
                {
                    actionContext.Packet.SetBodyBinary(this.Serializer, returnValue);
                    actionContext.Client.Send(actionContext.Packet);
                }
            }
            catch (Exception ex)
            {
                var exContext = new ExceptionContext(actionContext, ex);
                this.RaiseException(filters, exContext);
            }
            finally
            {
                // 释放上下文
                this.CurrentContext = null;
            }
        }

        /// <summary>
        /// 执行过滤器
        /// </summary>
        /// <param name="filters">过滤器</param>
        /// <param name="actionContext">上下文</param>   
        private void InvokeFiltersBefore(IEnumerable<IFilter> filters, ActionContext actionContext)
        {
            // OnAuthorization
            foreach (var globalFilter in GlobalFilters.FilterCollection.AuthorizationFilters)
            {
                globalFilter.OnAuthorization(actionContext);
            }
            this.OnAuthorization(actionContext);
            foreach (var filter in filters)
            {
                var authorizationFilter = filter as IAuthorizationFilter;
                if (authorizationFilter != null)
                {
                    authorizationFilter.OnAuthorization(actionContext);
                }
            }

            // OnExecuting
            foreach (var globalFilter in GlobalFilters.FilterCollection.ActionFilters)
            {
                globalFilter.OnExecuting(actionContext);
            }
            this.OnExecuting(actionContext);
            foreach (var filter in filters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuting(actionContext);
                }
            }
        }

        /// <summary>
        /// 执行过滤器
        /// </summary>
        /// <param name="filters">过滤器</param>
        /// <param name="actionContext">上下文</param>       
        private void InvokeFiltersAfter(IEnumerable<IFilter> filters, ActionContext actionContext)
        {
            // 全局过滤器
            foreach (var globalFilter in GlobalFilters.FilterCollection.ActionFilters)
            {
                globalFilter.OnExecuted(actionContext);
            }

            // 自身过滤器
            this.OnExecuted(actionContext);

            // 特性过滤器
            foreach (var filter in filters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuted(actionContext);
                }
            }
        }

        /// <summary>
        /// 并将异常传给客户端并调用OnException
        /// </summary>
        /// <param name="actionFilters">服务行为过滤器</param>
        /// <param name="exceptionContext">上下文</param>    
        private void RaiseException(IEnumerable<IFilter> actionFilters, ExceptionContext exceptionContext)
        {
            FastTcpCommon.RaiseRemoteException(exceptionContext, this.Serializer);

            foreach (var filter in GlobalFilters.FilterCollection.ExceptionFilters)
            {
                filter.OnException(exceptionContext);
            }

            this.OnException(exceptionContext);

            foreach (var filter in actionFilters)
            {
                var exceptionFilter = filter as IExceptionFilter;
                if (exceptionFilter != null)
                {
                    exceptionFilter.OnException(exceptionContext);
                }
            }
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="cmd">数据包的Action值</param>
        /// <param name="parameters">参数列表</param>
        /// <exception cref="RemoteException"></exception>
        protected void InvokeRemote(SocketAsync<FastPacket> client, int cmd, params object[] parameters)
        {
            FastTcpCommon.InvokeRemote(client, this.Serializer, cmd, parameters);
        }

        /// <summary>
        /// 将数据发送到远程端     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="cmd">数据包的命令值</param>
        /// <param name="parameters"></param>
        /// <exception cref="RemoteException"></exception>
        /// <returns>远程数据任务</returns>
        protected Task<T> InvokeRemote<T>(SocketAsync<FastPacket> client, int cmd, params object[] parameters)
        {
            return FastTcpCommon.InvokeRemote<T>(client, this.Serializer, cmd, parameters);
        }


        #region 过滤器接口实现
        /// <summary>
        /// 获取或设置排序
        /// </summary>
        public int Order
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// 是否允许多个实例
        /// </summary>
        public bool AllowMultiple
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 授权时触发       
        /// </summary>
        /// <param name="actionContext">上下文</param>       
        /// <returns></returns>
        public virtual void OnAuthorization(ActionContext actionContext)
        {
        }

        /// <summary>
        /// 在执行服务行为前触发       
        /// </summary>
        /// <param name="actionContext">上下文</param>       
        /// <returns></returns>
        public virtual void OnExecuting(ActionContext actionContext)
        {
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="exceptionContext">上下文</param>
        public virtual void OnException(ExceptionContext exceptionContext)
        {
        }
        /// <summary>
        /// 在执行服务行为后触发
        /// </summary>
        /// <param name="actionContext">上下文</param>      
        public virtual void OnExecuted(ActionContext actionContext)
        {
        }
        #endregion

        #region IDisponse成员
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
        ~FastServiceBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Serializer = null;
                this.GetFilters = null;
            }
        }
        #endregion
    }
}
