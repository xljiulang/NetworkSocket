using NetworkSocket.Core;
using NetworkSocket.Exceptions;
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
        public  TimeSpan TimeOut{get;set;}         

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
            this.apiActionList = new ApiActionList();
            this.PacketIdProvider = new PacketIdProvider();
            this.TaskSetActionTable = new TaskSetActionTable();

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
                this.apiActionList.AddRange(actions);
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
                .GetMethods()
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
            session.SetProtocolWrapper("websocket", jsonWebSocketSession);
        }

        /// <summary>
        /// 接收到文本信息时
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="content">内容</param>
        protected sealed override void OnText(IContenxt context, string content)
        {
            var jsonPacket = this.TryGetJsonPacket(context, content);
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
                this.ProcessRequest(requestContext);
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
                var exceptionConext = new ExceptionContext(requestContext, ex);
                this.ExecGlobalExceptionFilters(exceptionConext);
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
                exceptionContext.Session.UnWrap().SendText(packetJson);
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
    }
}
