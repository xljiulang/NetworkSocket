using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示基于Json文本协议通讯的WebSocket服务
    /// </summary>
    public class JsonWebSocketServer : WebSocketServerBase<JsonWebSocketSession>, IJsonWebSocketServer
    {
        /// <summary>
        /// 所有Api行为
        /// </summary>
        private ApiActionList apiActionList;

        /// <summary>
        /// 获取数据包ID生成提供者
        /// </summary>
        internal PacketIdProvider PacketIdProvider { get; private set; }

        /// <summary>
        /// 获取任务行为记录表
        /// </summary>
        internal TaskSetActionTable TaskSetActionTable { get; private set; }

        /// <summary>
        /// 获取或设置请求等待超时时间(毫秒)    
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int TimeOut
        {
            get
            {
                return this.TaskSetActionTable.TimeOut;
            }
            set
            {
                this.TaskSetActionTable.TimeOut = value;
            }
        }

        /// <summary>
        /// 获取或设置序列化工具       
        /// </summary>
        public IDynamicJsonSerializer JsonSerializer { get; set; }

        /// <summary>
        /// 获取全局过滤器管理者
        /// </summary>
        public IGlobalFilters GlobalFilters { get; private set; }

        /// <summary>
        /// 获取或设置依赖关系解析提供者
        /// 默认提供者解析为单例模式
        /// </summary>
        public IDependencyResolver DependencyResolver { get; set; }

        /// <summary>
        /// 获取或设置Api行为特性过滤器提供者
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// JsonWebSocket服务端
        /// </summary>
        public JsonWebSocketServer()
        {
            this.apiActionList = new ApiActionList();
            this.PacketIdProvider = new PacketIdProvider();
            this.TaskSetActionTable = new TaskSetActionTable();

            this.JsonSerializer = new DefaultDynamicJsonSerializer();
            this.GlobalFilters = new WebSocketGlobalFilters();
            this.DependencyResolver = new DefaultDependencyResolver();
            this.FilterAttributeProvider = new DefaultFilterAttributeProvider();
        }

        /// <summary>
        /// 绑定程序集下所有实现IJsonWebSocketApiService的服务
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public JsonWebSocketServer BindService(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException();
            }

            var apiServices = assembly.GetTypes().Where(item =>
                item.IsAbstract == false
                && item.IsInterface == false
                && typeof(IJsonWebSocketApiService).IsAssignableFrom(item));

            if (apiServices.Count() == 0)
            {
                throw new ArgumentException(string.Format("程序集{0}不包含任何{1}服务", assembly.GetName().Name, typeof(IJsonWebSocketApiService).Name));
            }

            return this.BindService(apiServices);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <typeparam name="T">Api服务类型</typeparam>             
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>  
        public JsonWebSocketServer BindService<T>() where T : IJsonWebSocketApiService
        {
            return this.BindService(typeof(T));
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="apiServiceType">Api服务类型</param>       
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public JsonWebSocketServer BindService(params Type[] apiServiceType)
        {
            return this.BindService((IEnumerable<Type>)apiServiceType);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="apiServiceType">Api服务类型</param>       
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public JsonWebSocketServer BindService(IEnumerable<Type> apiServiceType)
        {
            if (apiServiceType == null)
            {
                throw new ArgumentNullException("apiServiceType");
            }

            if (apiServiceType.Count() == 0)
            {
                throw new ArgumentOutOfRangeException("apiServiceType", "apiServiceType集合不能为空");
            }

            if (apiServiceType.Any(item => item == null))
            {
                throw new ArgumentException("apiServiceType不能含null值");
            }

            if (apiServiceType.Any(item => item.IsInterface || item.IsAbstract))
            {
                throw new ArgumentException("apiServiceType不能是接口或抽象类");
            }

            if (apiServiceType.Any(item => typeof(IJsonWebSocketApiService).IsAssignableFrom(item) == false))
            {
                throw new ArgumentException(string.Format("apiServiceType必须派生于{0}", typeof(IJsonWebSocketApiService).Name));
            }

            foreach (var type in apiServiceType)
            {
                var actions = this.GetServiceApiActions(type);
                this.apiActionList.AddRange(actions);
            }
            return this;
        }


        /// <summary>
        /// 获取服务类型的Api行为
        /// </summary>
        /// <param name="seviceType">服务类型</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        private IEnumerable<ApiAction> GetServiceApiActions(Type seviceType)
        {
            return seviceType
                .GetMethods()
                .Where(item => Attribute.IsDefined(item, typeof(ApiAttribute)))
                .Select(method => new ApiAction(method));
        }

        /// <summary>
        /// 创建新的会话对象
        /// </summary>
        /// <returns></returns>
        protected sealed override JsonWebSocketSession OnCreateSession()
        {
            return new JsonWebSocketSession(this);
        }

        /// <summary>
        /// 接收到文本信息时
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="content">内容</param>
        protected sealed override void OnText(JsonWebSocketSession session, string content)
        {
            var packet = this.TryGetJsonPacket(session, content);
            if (packet == null)
            {
                return;
            }

            var requestContext = new RequestContext(session, packet, this.AllSessions);
            if (packet.state == false)
            {
                this.ProcessRemoteException(requestContext);
            }
            else
            {
                this.ProcessRequest(requestContext);
            }
        }


        /// <summary>
        /// 尝试获取数据包
        /// </summary>     
        /// <param name="session">会话</param>
        /// <param name="content">内容</param>        
        /// <returns></returns>
        private JsonPacket TryGetJsonPacket(JsonWebSocketSession session, string content)
        {
            try
            {
                var packet = this.JsonSerializer.Deserialize(content);
                var jsonPacket = new JsonPacket
                {
                    api = packet.api,
                    id = packet.id ?? 0,
                    state = packet.state ?? true,
                    fromClient = packet.fromClient ?? true,
                    body = packet.body
                };
                return jsonPacket;
            }
            catch (ProtocolException ex)
            {
                this.OnException(session, ex);
                return null;
            }
            catch (Exception ex)
            {
                this.OnException(session, new ProtocolException(ex.Message));
                return null;
            }
        }

        /// <summary>
        /// 处理远返回的程异常
        /// </summary>
        /// <param name="requestContext">请求上下文</param>     
        private void ProcessRemoteException(RequestContext requestContext)
        {
            var taskSetAction = this.TaskSetActionTable.Take(requestContext.Packet.id);
            if (taskSetAction == null)
            {
                return;
            }

            var body = requestContext.Packet.body;
            var message = body == null ? null : body.ToString();
            var exception = new RemoteException(message);
            taskSetAction.SetException(exception);
        }

        /// <summary>
        /// 处理正常的数据请求
        /// </summary>
        /// <param name="requestContext">请求上下文</param>       
        private void ProcessRequest(RequestContext requestContext)
        {
            if (requestContext.Packet.fromClient == false)
            {
                this.SetApiActionTaskResult(requestContext);
                return;
            }

            var action = this.GetApiAction(requestContext);
            if (action == null)
            {
                return;
            }

            var actionContext = new ActionContext(requestContext, action);
            var jsonWebSocketApiService = this.GetJsonWebSocketApiService(actionContext);
            if (jsonWebSocketApiService == null)
            {
                return;
            }

            // 执行Api行为           
            jsonWebSocketApiService.Execute(actionContext);
            this.DependencyResolver.TerminateService(jsonWebSocketApiService);
        }

        /// <summary>
        /// 设置Api行为返回的任务结果
        /// </summary>
        /// <param name="requestContext">上下文</param>      
        /// <returns></returns>
        private bool SetApiActionTaskResult(RequestContext requestContext)
        {
            var taskSetAction = this.TaskSetActionTable.Take(requestContext.Packet.id);
            if (taskSetAction == null)
            {
                return true;
            }

            try
            {
                var body = requestContext.Packet.body;
                var value = this.JsonSerializer.Convert(body, taskSetAction.ValueType);
                return taskSetAction.SetResult(value);
            }
            catch (SerializerException ex)
            {
                return taskSetAction.SetException(ex);
            }
            catch (Exception ex)
            {
                return taskSetAction.SetException(new SerializerException(ex));
            }
        }


        /// <summary>
        /// 获取Api行为
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        private ApiAction GetApiAction(RequestContext requestContext)
        {
            var action = this.apiActionList.TryGet(requestContext.Packet.api);
            if (action == null)
            {
                var exception = new ApiNotExistException(requestContext.Packet.api);
                var exceptionContext = new ExceptionContext(requestContext, exception);
                this.SendRemoteException(exceptionContext);
                this.ExecGlobalExceptionFilters(exceptionContext);
            }
            return action;
        }

        /// <summary>
        /// 获取JsonWebSocketApiService实例
        /// </summary>
        /// <param name="actionContext">Api行为上下文</param>
        /// <returns></returns>
        private IJsonWebSocketApiService GetJsonWebSocketApiService(ActionContext actionContext)
        {
            try
            {
                var serviceType = actionContext.Action.DeclaringService;
                var instance = this.DependencyResolver.GetService(serviceType);
                return instance as IJsonWebSocketApiService;
            }
            catch (Exception ex)
            {
                var resolveException = new ResolveException(actionContext.Action.DeclaringService, ex);
                var exceptionContext = new ExceptionContext(actionContext, resolveException);
                this.SendRemoteException(exceptionContext);
                this.ExecGlobalExceptionFilters(exceptionContext);
                return null;
            }
        }

        /// <summary>       
        /// 将异常信息发送到远程端
        /// </summary>
        /// <param name="exceptionContext">上下文</param>       
        /// <returns></returns>
        internal bool SendRemoteException(ExceptionContext exceptionContext)
        {
            try
            {
                var packet = exceptionContext.Packet;
                packet.state = false;
                packet.body = exceptionContext.Exception.Message;

                var packetJson = this.JsonSerializer.Serialize(packet);
                exceptionContext.Session.SendText(packetJson);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 执行异常过滤器
        /// </summary>         
        /// <param name="exceptionContext">上下文</param>       
        private void ExecGlobalExceptionFilters(ExceptionContext exceptionContext)
        {
            if (this.GlobalFilters.Count == 0)
            {
                return;
            }

            foreach (IFilter filter in this.GlobalFilters)
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
        /// 异常时
        /// </summary>
        /// <param name="session">产生异常的会话</param>
        /// <param name="exception">异常</param>
        protected sealed override void OnException(JsonWebSocketSession session, Exception exception)
        {
            var requestContext = new RequestContext(session, null, this.AllSessions);
            var exceptionConext = new ExceptionContext(requestContext, exception);
            this.ExecGlobalExceptionFilters(exceptionConext);
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
                this.apiActionList = null;

                this.TaskSetActionTable.Clear();
                this.TaskSetActionTable = null;

                this.PacketIdProvider = null;
                this.JsonSerializer = null;
                this.GlobalFilters = null;
                this.DependencyResolver = null;
                this.FilterAttributeProvider = null;
            }
        }
        #endregion
    }
}
