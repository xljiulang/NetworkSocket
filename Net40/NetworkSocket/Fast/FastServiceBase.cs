using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using NetworkSocket.Fast.Internal;
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
    /// </summary>
    public abstract class FastServiceBase : IFastService, IAuthorizationFilter, IActionFilter, IExceptionFilter
    {
        /// <summary>
        /// 线程唯一上下文
        /// </summary>
        [ThreadStatic]
        private static ActionContext currentContext;

        /// <summary>
        /// 获取或设置关联的TcpServer
        /// </summary>
        public IFastTcpServer FastTcpServer { get; set; }

        /// <summary>
        /// 获取当前服务行为上下文
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
        /// 执行服务行为
        /// </summary>
        /// <param name="actionContext">上下文</param>      
        public void Execute(ActionContext actionContext)
        {
            // 如果是Cmd值对应是Self类型方法 也就是客户端主动调用服务行为
            if (actionContext.Action.Implement == Implements.Self)
            {
                this.TryExecuteAction(actionContext);
            }
            else
            {
                FastTcpCommon.RaiseTaskResult(actionContext);
            }
        }

        /// <summary>
        /// 调用自身实现的服务行为
        /// 将返回值发送给客户端        
        /// </summary>       
        /// <param name="actionContext">上下文</param>  
        private void TryExecuteAction(ActionContext actionContext)
        {
            var filters = this.FastTcpServer.FilterAttributeProvider.GetActionFilters(actionContext.Action);

            try
            {
                this.CurrentContext = actionContext;
                this.ExecuteAction(actionContext, filters);
            }
            catch (Exception exception)
            {
                var exceptionContext = new ExceptionContext(actionContext, exception);
                this.ExecExceptionFilters(filters, exceptionContext);
                if (exceptionContext.ExceptionHandled == false)
                {
                    throw;
                }
            }
            finally
            {
                this.CurrentContext = null;
            }
        }

        /// <summary>
        /// 调用自身实现的服务行为
        /// 将返回值发送给客户端        
        /// </summary>       
        /// <param name="actionContext">上下文</param>       
        /// <param name="filters">过滤器</param>
        private void ExecuteAction(ActionContext actionContext, IEnumerable<IFilter> filters)
        {
            // 执行Filter
            this.ExecFiltersBeforeAction(filters, actionContext);

            var parameters = FastTcpCommon.GetActionParameters(actionContext, this.FastTcpServer.Serializer);
            var returnValue = actionContext.Action.Execute(this, parameters);

            // 执行Filter
            this.ExecFiltersAfterAction(filters, actionContext);

            // 返回数据
            if (actionContext.Action.IsVoidReturn == false && actionContext.Client.IsConnected)
            {
                actionContext.Packet.SetBodyBinary(this.FastTcpServer.Serializer, returnValue);
                actionContext.Client.Send(actionContext.Packet);
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
            FastTcpCommon.InvokeRemote(client, this.FastTcpServer.Serializer, cmd, parameters);
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
            return FastTcpCommon.InvokeRemote<T>(client, this.FastTcpServer.Serializer, cmd, parameters);
        }

        #region IFilter
        /// <summary>
        /// 获取或设置排序
        /// </summary>
        public int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 是否允许多个实例
        /// </summary>
        public bool AllowMultiple
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
        public virtual void OnAuthorization(ActionContext filterContext)
        {
        }

        /// <summary>
        /// 在执行服务行为前触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        public virtual void OnExecuting(ActionContext filterContext)
        {
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="filterContext">上下文</param>
        public virtual void OnException(ExceptionContext filterContext)
        {
        }
        /// <summary>
        /// 在执行服务行为后触发
        /// </summary>
        /// <param name="filterContext">上下文</param>      
        public virtual void OnExecuted(ActionContext filterContext)
        {
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
                this.FastTcpServer = null;
            }
        }
        #endregion
    }
}
