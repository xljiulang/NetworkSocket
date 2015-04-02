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
    /// 所有自身实现的服务方法的第一个参数是ActionContext类型
    /// </summary>
    public abstract class FastServiceBase : IAuthorizationFilter, IActionFilter, IExceptionFilter, IDisposable
    {
        /// <summary>
        /// 线程唯一上下文
        /// </summary>
        [ThreadStatic]
        private static ActionContext currentContext;

        /// <summary>
        /// 获取或设置序列化工具
        /// </summary>
        internal ISerializer Serializer;

        /// <summary>
        /// 服务行为特性过滤器提供者
        /// </summary>
        internal IFilterAttributeProvider FilterAttributeProvider;

        /// <summary>
        /// 获取当前服务行为上下文
        /// </summary>
        public ActionContext CurrentContext
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
            var filters = this.FilterAttributeProvider.GetActionFilters(actionContext.Action);

            try
            {
                // 设置上下文
                this.CurrentContext = actionContext;
                this.ExecFiltersBeforeAction(filters, actionContext);

                var parameters = FastTcpCommon.GetActionParameters(actionContext, this.Serializer);
                var returnValue = actionContext.Action.Execute(this, parameters);

                // 执行Filter
                this.ExecFiltersAfterAction(filters, actionContext);

                if (actionContext.Action.IsVoidReturn == false && actionContext.Client.IsConnected)
                {
                    actionContext.Packet.SetBodyBinary(this.Serializer, returnValue);
                    actionContext.Client.Send(actionContext.Packet);
                }
            }
            catch (Exception ex)
            {
                var exceptionContext = new ExceptionContext(actionContext, ex);
                this.ExecExceptionFilters(filters, exceptionContext);
                if (exceptionContext.ExceptionHandled == false)
                {
                    throw;
                }
            }
            finally
            {
                // 释放上下文
                this.CurrentContext = null;
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
                this.Serializer = null;
                this.FilterAttributeProvider = null;
            }
        }
        #endregion
    }
}
