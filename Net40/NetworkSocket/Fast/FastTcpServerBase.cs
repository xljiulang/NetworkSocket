using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp服务端抽象类 
    /// </summary>
    public abstract class FastTcpServerBase : TcpServerBase<FastPacket>
    {
        /// <summary>
        /// 所有服务行为
        /// </summary>
        private FastActionList fastActionList;

        /// <summary>
        /// 数据包哈希码提供者
        /// </summary>
        private HashCodeProvider hashCodeProvider;

        /// <summary>
        /// 任务行为表
        /// </summary>
        private TaskSetActionTable taskSetActionTable;

        /// <summary>
        /// 获取或设置请求超时时间
        /// 单位毫秒
        /// </summary>
        public int TimeOut
        {
            get
            {
                return this.taskSetActionTable.TimeOut;
            }
            set
            {
                this.taskSetActionTable.TimeOut = value;
            }
        }

        /// <summary>
        /// 获取或设置序列化工具
        /// 默认是Json序列化
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 获取或设置服务行为特性过滤器提供者
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 快速构建Tcp服务端
        /// </summary>
        public FastTcpServerBase()
        {
            this.fastActionList = new FastActionList();
            this.hashCodeProvider = new HashCodeProvider();
            this.taskSetActionTable = new TaskSetActionTable();

            this.Serializer = new DefaultSerializer();
            this.FilterAttributeProvider = new FilterAttributeProvider();
        }

        /// <summary>
        /// 绑定本程序集所有实现IFastService的服务
        /// </summary>
        /// <returns></returns>       
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService()
        {
            var allServices = this.GetType().Assembly.GetTypes().Where(item => typeof(IFastService).IsAssignableFrom(item));
            return this.BindService(allServices);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>       
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService<T>() where T : IFastService
        {
            return this.BindService(typeof(T));
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService(params Type[] serviceType)
        {
            return this.BindService((IEnumerable<Type>)serviceType);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="serivceType">服务类型</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FastTcpServerBase BindService(IEnumerable<Type> serivceType)
        {
            if (serivceType == null)
            {
                throw new ArgumentNullException("serivceType");
            }

            if (serivceType.Any(item => item == null))
            {
                throw new ArgumentException("serivceType不能含null值");
            }

            if (serivceType.Any(item => typeof(IFastService).IsAssignableFrom(item) == false))
            {
                throw new ArgumentException("serivceType必须派生于IFastService");
            }

            foreach (var type in serivceType)
            {
                var actions = FastTcpCommon.GetServiceFastActions(type);
                this.fastActionList.AddRange(actions);
            }
            return this;
        }

        /// <summary>
        /// 获取服务实例
        /// 并赋值给服务实例的FastTcpServer属性
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        public T GetService<T>() where T : IFastService
        {
            return (T)this.GetService(typeof(T));
        }

        /// <summary>
        /// 获取服务实例
        /// 并赋值给服务实例的FastTcpServer属性
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        private IFastService GetService(Type serviceType)
        {
            var fastService = DependencyResolver.Current.GetService(serviceType) as IFastService;
            return fastService;
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="command">数据包的command值</param>
        /// <param name="parameters">参数列表</param>    
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception>         
        internal void InvokeRemote(SocketAsync<FastPacket> client, int command, params object[] parameters)
        {
            var hashCode = this.hashCodeProvider.GetHashCode();
            var packet = new FastPacket(command, hashCode);
            packet.SetBodyBinary(this.Serializer, parameters);
            client.Send(packet);
        }

        /// <summary>
        /// 将数据发送到远程端     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="command">数据包的命令值</param>
        /// <param name="parameters">参数</param>     
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception> 
        /// <exception cref="RemoteException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <returns>远程数据任务</returns>  
        internal Task<T> InvokeRemote<T>(SocketAsync<FastPacket> client, int command, params object[] parameters)
        {
            var hashCode = this.hashCodeProvider.GetHashCode();
            return FastTcpCommon.InvokeRemote<T>(client, this.taskSetActionTable, this.Serializer, command, hashCode, parameters);
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法
        /// 此方法用于处理和分析收到的数据
        /// 如果得到一个数据包，将触发OnRecvComplete方法
        /// [注]这里只需处理一个数据包的流程
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="recvBuilder">接收到的历史数据</param>
        /// <returns>如果不够一个数据包，则请返回null</returns>
        protected override FastPacket OnReceive(SocketAsync<FastPacket> client, ByteBuilder recvBuilder)
        {
            return FastPacket.From(recvBuilder);
        }

        /// <summary>
        /// 当接收到客户端数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        protected override void OnRecvComplete(SocketAsync<FastPacket> client, FastPacket packet)
        {
            var requestContext = new RequestContext { Client = client, Packet = packet };

            if (packet.IsException)
            {
                this.ProcessRemoteException(requestContext);
            }
            else
            {
                this.ProcessRequest(requestContext);
            }
        }

        /// <summary>
        /// 处理远返回的程异常
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        private void ProcessRemoteException(RequestContext requestContext)
        {
            var exceptionContext = FastTcpCommon.SetFastActionTaskException(this.Serializer, this.taskSetActionTable, requestContext);
            if (exceptionContext == null)
            {
                return;
            }

            this.ExecExceptionFilters(exceptionContext);
            if (exceptionContext.ExceptionHandled == false)
            {
                throw exceptionContext.Exception;
            }
        }

        /// <summary>
        /// 处理正常的数据请求
        /// </summary>
        /// <param name="requestContext">请求上下文</param>       
        private void ProcessRequest(RequestContext requestContext)
        {
            var action = this.GetFastAction(requestContext);
            if (action == null)
            {
                return;
            }

            if (action.Implement == Implements.Remote)
            {
                FastTcpCommon.SetFastActionTaskResult(requestContext, this.taskSetActionTable);
                return;
            }

            var actionContext = new ActionContext(requestContext, action);
            var fastService = this.GetFastService(actionContext);
            if (fastService == null)
            {
                return;
            }

            // 执行服务行为          
            fastService.Execute(this, actionContext);

            // 释放资源
            DependencyResolver.Current.TerminateService(fastService);
        }

        /// <summary>
        /// 获取服务行为
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        private FastAction GetFastAction(RequestContext requestContext)
        {
            var action = this.fastActionList.TryGet(requestContext.Packet.Command);
            if (action != null)
            {
                return action;
            }

            var exception = new ActionNotImplementException(requestContext.Packet.Command);
            var exceptionContext = new ExceptionContext(requestContext, exception);

            FastTcpCommon.SetRemoteException(this.Serializer, exceptionContext);
            this.ExecExceptionFilters(exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exception;
            }

            return null;
        }

        /// <summary>
        /// 获取FastService实例
        /// </summary>
        /// <param name="actionContext">服务行为上下文</param>
        /// <returns></returns>
        private IFastService GetFastService(ActionContext actionContext)
        {
            // 获取服务实例
            var fastService = this.GetService(actionContext.Action.DeclaringService);
            if (fastService != null)
            {
                return fastService;
            }
            var exception = new Exception(string.Format("无法获取类型{0}的实例", actionContext.Action.DeclaringService));
            var exceptionContext = new ExceptionContext(actionContext, exception);

            FastTcpCommon.SetRemoteException(this.Serializer, exceptionContext);
            this.ExecExceptionFilters(exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exception;
            }

            return null;
        }


        /// <summary>
        /// 执行异常过滤器
        /// </summary>         
        /// <param name="exceptionContext">上下文</param>       
        private void ExecExceptionFilters(ExceptionContext exceptionContext)
        {
            foreach (var filter in GlobalFilters.ExceptionFilters)
            {
                if (exceptionContext.ExceptionHandled == false)
                {
                    filter.OnException(exceptionContext);
                }
            }
        }

        #region IDisponse
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.fastActionList = null;

                this.taskSetActionTable.Clear();
                this.taskSetActionTable = null;

                this.hashCodeProvider = null;
                this.Serializer = null;
                this.FilterAttributeProvider = null;
            }
        }
        #endregion
    }
}
