using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Tasks;
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
    /// 表示基于Json文本协议通讯的WebSocket中间件
    /// </summary>
    public class JsonWebSocketMiddleware : WebSocketMiddlewareBase, IDependencyResolverSupportable, IFilterSupportable
    {
        /// <summary>
        /// 所有Api行为
        /// </summary>
        private ApiActionTable apiActionTable;

        /// <summary>
        /// 获取数据包ID生成提供者
        /// </summary>
        internal PacketIdProvider PacketIdProvider { get; private set; }

        /// <summary>
        /// 获取任务行为记录表
        /// </summary>
        internal TaskSetterTable<long> TaskSetterTable { get; private set; }

        /// <summary>
        /// 获取或设置请求等待超时时间(毫秒)    
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TimeSpan TimeOut { get; set; }

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
        /// JsonWebSocket中间件
        /// </summary>
        public JsonWebSocketMiddleware()
        {
            this.apiActionTable = new ApiActionTable();
            this.PacketIdProvider = new PacketIdProvider();
            this.TaskSetterTable = new TaskSetterTable<long>();

            this.TimeOut = TimeSpan.FromSeconds(30);
            this.JsonSerializer = new DefaultDynamicJsonSerializer();
            this.GlobalFilters = new WebSocketGlobalFilters();
            this.DependencyResolver = new DefaultDependencyResolver();
            this.FilterAttributeProvider = new DefaultFilterAttributeProvider();

            DomainAssembly.GetAssemblies().ForEach(item => this.BindService(item));
        }

        /// <summary>
        /// 绑定程序集下所有实现的服务
        /// </summary>
        /// <param name="assembly">程序集</param>
        private void BindService(Assembly assembly)
        {
            var jsonWebSockeApiServices = assembly
                .GetTypes()
                .Where(item =>
                    item.IsAbstract == false
                    && item.IsInterface == false
                    && typeof(IJsonWebSocketApiService).IsAssignableFrom(item));

            foreach (var type in jsonWebSockeApiServices)
            {
                var actions = this.GetServiceApiActions(type);
                this.apiActionTable.AddRange(actions);
            }
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
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Where(item => Attribute.IsDefined(item, typeof(ApiAttribute)))
                .Select(method => new ApiAction(method));
        }

        /// <summary>
        /// 设置会话的包装对象
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="wrapper">包装对象</param>
        protected override void OnSetProtocolWrapper(ISession session, WebSocketSession wrapper)
        {
            var jsonWebSocketSession = new JsonWebSocketSession(this, wrapper);
            session.SetProtocolWrapper(Protocol.WebSocket, jsonWebSocketSession);
        }

        /// <summary>
        /// 接收到文本信息时
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="frame">数据帧</param>
        protected sealed override async void OnText(IContenxt context, FrameRequest frame)
        {
            var text = Encoding.UTF8.GetString(frame.Content);
            var jsonPacket = this.TryGetJsonPacket(context, text);
            if (jsonPacket == null)
            {
                return;
            }

            var session = (JsonWebSocketSession)context.Session.Wrapper;
            var requestContext = new RequestContext(session, jsonPacket, context.AllSessions);
            if (jsonPacket.state == false)
            {
                this.ProcessRemoteException(requestContext);
            }
            else
            {
                await this.ProcessRequestAsync(requestContext);
            }
        }


        /// <summary>
        /// 尝试获取数据包
        /// </summary>     
        /// <param name="context">上下文</param>
        /// <param name="content">内容</param>        
        /// <returns></returns>
        private JsonPacket TryGetJsonPacket(IContenxt context, string content)
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
            catch (Exception ex)
            {
                var session = (JsonWebSocketSession)context.Session.Wrapper;
                var requestContext = new RequestContext(session, null, context.AllSessions);
                this.OnException(requestContext, ex);
                return null;
            }
        }

        /// <summary>
        /// 处理远返回的程异常
        /// </summary>
        /// <param name="requestContext">请求上下文</param>     
        private void ProcessRemoteException(RequestContext requestContext)
        {
            var taskSetter = this.TaskSetterTable.Take(requestContext.Packet.id);
            if (taskSetter == null)
            {
                return;
            }

            var body = requestContext.Packet.body;
            var message = body == null ? null : body.ToString();
            var exception = new RemoteException(message);
            taskSetter.SetException(exception);
        }

        /// <summary>
        /// 处理正常的数据请求
        /// </summary>
        /// <param name="requestContext">请求上下文</param>  
        /// <returns></returns>
        private async Task ProcessRequestAsync(RequestContext requestContext)
        {
            if (requestContext.Packet.fromClient == false)
            {
                this.SetApiActionTaskResult(requestContext);
            }
            else
            {
                await this.TryExecuteRequestAsync(requestContext);
            }
        }


        /// <summary>
        /// 执行请求
        /// </summary>
        /// <param name="requestContext">上下文</param>
        /// <returns></returns>
        private async Task TryExecuteRequestAsync(RequestContext requestContext)
        {
            try
            {
                var action = this.GetApiAction(requestContext);
                var actionContext = new ActionContext(requestContext, action);
                var jsonWebSocketApiService = this.GetJsonWebSocketApiService(actionContext);
                await jsonWebSocketApiService.ExecuteAsync(actionContext);
                this.DependencyResolver.TerminateService(jsonWebSocketApiService);
            }
            catch (Exception ex)
            {
                this.OnException(requestContext, ex);
            }
        }

        /// <summary>
        /// 设置Api行为返回的任务结果
        /// </summary>
        /// <param name="requestContext">上下文</param>      
        /// <returns></returns>
        private bool SetApiActionTaskResult(RequestContext requestContext)
        {
            var taskSetter = this.TaskSetterTable.Take(requestContext.Packet.id);
            if (taskSetter == null)
            {
                return true;
            }

            try
            {
                var body = requestContext.Packet.body;
                var value = this.JsonSerializer.Convert(body, taskSetter.ValueType);
                return taskSetter.SetResult(value);
            }
            catch (SerializerException ex)
            {
                return taskSetter.SetException(ex);
            }
            catch (Exception ex)
            {
                return taskSetter.SetException(new SerializerException(ex));
            }
        }


        /// <summary>
        /// 获取Api行为
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <exception cref="ApiNotExistException"></exception>
        /// <returns></returns>
        private ApiAction GetApiAction(RequestContext requestContext)
        {
            var action = this.apiActionTable.TryGetAndClone(requestContext.Packet.api);
            if (action == null)
            {
                throw new ApiNotExistException(requestContext.Packet.api);
            }
            return action;
        }

        /// <summary>
        /// 获取JsonWebSocketApiService实例
        /// </summary>
        /// <param name="actionContext">Api行为上下文</param>
        /// <exception cref="ResolveException"></exception>
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
                throw new ResolveException(actionContext.Action.DeclaringService, ex);
            }
        }

        /// <summary>
        /// 异常发生时
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="exception">异常</param>
        protected virtual void OnException(RequestContext context, Exception exception)
        {
            if (context.Packet != null)
            {
                this.SendRemoteException(context, exception);
            }
        }

        /// <summary>       
        /// 将异常信息发送到远程端
        /// </summary>
        /// <param name="context">上下文</param>       
        /// <param name="exception">异常</param>
        /// <returns></returns>
        internal bool SendRemoteException(RequestContext context, Exception exception)
        {
            try
            {
                var packet = context.Packet;
                packet.state = false;
                packet.body = exception.Message;

                var packetJson = this.JsonSerializer.Serialize(packet);
                context.Session.UnWrap().SendText(packetJson);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
