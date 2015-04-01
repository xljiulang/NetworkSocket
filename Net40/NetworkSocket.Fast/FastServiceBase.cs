using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using NetworkSocket.Fast.Methods;
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
    /// 要求所有服务从此类派生
    /// </summary>
    public abstract class FastServiceBase : IDisposable
    {
        /// <summary>
        /// 获取或设置序列化工具
        /// </summary>
        internal ISerializer Serializer;

        /// <summary>
        /// 获取过滤委托
        /// </summary>
        internal Func<MethodInfo, IEnumerable<Filter>> GetFilters;

        /// <summary>
        /// 处理封包
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">封包</param>
        /// <param name="method">服务方法</param>
        internal void ProcessPacket(SocketAsync<FastPacket> client, FastPacket packet, ServiceMethod method)
        {
            // 如果是Cmd值对应是Self类型方法 也就是客户端主动调用服务方法
            if (method.ServiceAttribute.Implement == Implements.Self)
            {
                this.TryInvoke(method, client, packet);
                return;
            }

            // 如果是收到返回值 从回调表找出相关回调来调用
            var callBack = CallbackTable.Take(packet.HashCode);
            if (callBack != null)
            {
                var returnBytes = packet.GetBodyParameter().FirstOrDefault();
                callBack(packet.IsException, returnBytes);
            }
        }


        /// <summary>
        /// 调用自身方法
        /// 将返回值发送给客户端
        /// 或将异常发送给客户端
        /// </summary>       
        /// <param name="method">方法</param>
        /// <param name="client">客户端对象</param>
        /// <param name="packet">数据</param>
        private void TryInvoke(ServiceMethod method, SocketAsync<FastPacket> client, FastPacket packet)
        {
            try
            {
                // 执行Filter
                var filters = this.GetFilters(method.Method);
                this.InvokeFiltersBefore(filters, client, packet);

                var parameters = FastTcpCommon.GetServiceMethodParameters(method, packet, this.Serializer, client);
                var returnValue = method.Invoke(this, parameters);

                // 执行Filter
                this.InvokeFiltersAfter(filters, client, packet);

                if (method.HasReturn && client.IsConnected)
                {
                    packet.SetBodyBinary(this.Serializer, returnValue);
                    client.Send(packet);
                }
            }
            catch (Exception ex)
            {
                this.RaiseException(client, packet, ex);
            }
        }


        /// <summary>
        /// 执行过滤器
        /// </summary>
        /// <param name="filters">过滤器</param>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        private void InvokeFiltersBefore(IEnumerable<Filter> filters, SocketAsync<FastPacket> client, FastPacket packet)
        {
            foreach (var filter in filters)
            {
                switch (filter.FilterScope)
                {
                    case FilterScope.Authorization:
                        ((IAuthorizationFilter)filter.Instance).OnAuthorization(client, packet);
                        break;

                    default:
                        ((IActionFilter)filter.Instance).OnExecuting(client, packet);
                        break;
                }
            }
        }

        /// <summary>
        /// 执行过滤器
        /// </summary>
        /// <param name="filters">过滤器</param>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        public void InvokeFiltersAfter(IEnumerable<Filter> filters, SocketAsync<FastPacket> client, FastPacket packet)
        {
            foreach (var filter in filters)
            {
                if (filter.FilterScope != FilterScope.Authorization)
                {
                    ((IActionFilter)filter.Instance).OnExecuted(client, packet);
                }
            }
        }

        /// <summary>
        /// 并将异常传给客户端并调用OnException
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">封包</param>
        /// <param name="exception">异常</param>         
        private void RaiseException(SocketAsync<FastPacket> client, FastPacket packet, Exception exception)
        {
            FastTcpCommon.RaiseRemoteException(client, packet, exception, this.Serializer);
            this.OnException(client, exception);
        }

        /// <summary>
        /// 当操作中遇到处理异常时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="exception">异常</param>
        protected virtual void OnException(SocketAsync<FastPacket> client, Exception exception)
        {
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
            }
        }
        #endregion
    }
}
