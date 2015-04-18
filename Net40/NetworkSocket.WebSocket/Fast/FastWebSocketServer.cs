using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 表示基于Json文本协议通讯的WebSocket服务
    /// </summary>
    public class FastWebSocketServer : WebSocketServerBase, IFastWebSocketServer
    {
        /// <summary>
        /// 所有Api行为
        /// </summary>
        private ApiActionList apiActionList;

        /// <summary>
        /// 数据包哈希码提供者
        /// </summary>
        private PacketIdProvider packetIdProvider;

        /// <summary>
        /// 任务行为表
        /// </summary>
        private TaskSetActionTable taskSetActionTable;

        /// <summary>
        /// 获取或设置请求等待超时时间(毫秒)    
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        public IJsonSerializer JsonSerializer { get; set; }

        /// <summary>
        /// 获取或设置Api行为特性过滤器提供者
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 快速构建Tcp服务端
        /// </summary>
        public FastWebSocketServer()
        {
            this.apiActionList = new ApiActionList();
            this.packetIdProvider = new PacketIdProvider();
            this.taskSetActionTable = new TaskSetActionTable();

            this.JsonSerializer = new DefaultJsonSerializer();
            this.FilterAttributeProvider = new FilterAttributeProvider();
        }

        /// <summary>
        /// 绑定本程序集所有实现IJsonApiService的服务
        /// </summary>
        /// <returns></returns>       
        /// <exception cref="ArgumentException"></exception>
        public FastWebSocketServer BindService()
        {
            var allServices = this.GetType().Assembly.GetTypes().Where(item => typeof(IFastApiService).IsAssignableFrom(item));
            return this.BindService(allServices);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>       
        /// <exception cref="ArgumentException"></exception>
        public FastWebSocketServer BindService<T>() where T : IFastApiService
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
        public FastWebSocketServer BindService(params Type[] serviceType)
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
        public FastWebSocketServer BindService(IEnumerable<Type> serivceType)
        {
            if (serivceType == null)
            {
                throw new ArgumentNullException("serivceType");
            }

            if (serivceType.Any(item => item == null))
            {
                throw new ArgumentException("serivceType不能含null值");
            }

            if (serivceType.Any(item => typeof(IFastApiService).IsAssignableFrom(item) == false))
            {
                throw new ArgumentException("serivceType必须派生于IJsonApiService");
            }

            foreach (var type in serivceType)
            {
                var actions = FastWebSocketCommon.GetServiceApiActions(type);
                this.apiActionList.AddRange(actions);
            }
            return this;
        }


        /// <summary>
        /// 调用客户端实现的服务方法        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="api">api(区分大小写)</param>
        /// <param name="parameters">参数列表</param>    
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception>         
        public Task InvokeApi(IClient<Response> client, string api, params object[] parameters)
        {
            return Task.Factory.StartNew(() =>
            {
                var id = this.packetIdProvider.GetId();
                var packet = new FastPacket
                {
                    api = api,
                    id = id,
                    state = true,
                    fromClient = false,
                    body = parameters
                };
                var packetJson = this.JsonSerializer.Serialize(packet);
                client.Send(packetJson);
            });
        }

        /// <summary>
        /// 调用客户端实现的服务方法     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="api">api(区分大小写)</param>
        /// <param name="parameters">参数</param>     
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception> 
        /// <exception cref="RemoteException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <returns>远程数据任务</returns>  
        public Task<T> InvokeApi<T>(IClient<Response> client, string api, params object[] parameters)
        {
            var id = this.packetIdProvider.GetId();
            var taskSource = new TaskCompletionSource<T>();
            var packet = new FastPacket
            {
                api = api,
                id = id,
                state = true,
                fromClient = false,
                body = parameters
            };
            var packetJson = this.JsonSerializer.Serialize(packet);

            // 登记TaskSetAction           
            Action<SetTypes, string> setAction = (setType, json) =>
            {
                if (setType == SetTypes.SetReturnReult)
                {
                    if (json == null || json.Length == 0)
                    {
                        taskSource.TrySetResult(default(T));
                    }
                    else
                    {
                        var result = (T)this.JsonSerializer.Deserialize(json, typeof(T));
                        taskSource.TrySetResult(result);
                    }
                }
                else if (setType == SetTypes.SetReturnException)
                {
                    var exception = new RemoteException(json);
                    taskSource.TrySetException(exception);
                }
                else if (setType == SetTypes.SetTimeoutException)
                {
                    var exception = new TimeoutException();
                    taskSource.TrySetException(exception);
                }
                else if (setType == SetTypes.SetShutdownException)
                {
                    var exception = new SocketException(SocketError.Shutdown.GetHashCode());
                    taskSource.TrySetException(exception);
                }
            };
            var taskSetAction = new TaskSetAction(setAction);
            taskSetActionTable.Add(packet.id, taskSetAction);

            client.Send(packetJson);
            return taskSource.Task;
        }

        /// <summary>
        /// 获取数据包
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        private FastPacket GetFastPacket(IClient<Response> client, string content)
        {
            try
            {
                dynamic packet = JObject.Parse(content);
                var fastPacket = new FastPacket
                {
                    api = packet.api,
                    id = packet.id ?? 0,
                    state = packet.state ?? true,
                    fromClient = packet.fromClient ?? true,
                    body = packet.body
                };
                return fastPacket;
            }
            catch (Exception)
            {
                client.NormalClose(StatusCodes.ProtocolError);
                return null;
            }
        }

        /// <summary>
        /// 接收到文本信息时
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">内容</param>
        protected override void OnText(IClient<Response> client, string content)
        {
            var packet = this.GetFastPacket(client, content);
            if (packet == null)
            {
                return;
            }

            var requestContext = new RequestContext
            {
                WebSocketServer = this,
                Client = client,
                Packet = packet,
            };

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
        /// 处理远返回的程异常
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        private void ProcessRemoteException(RequestContext requestContext)
        {
            var remoteException = FastWebSocketCommon.SetApiActionTaskException(this.JsonSerializer, this.taskSetActionTable, requestContext);
            if (remoteException == null)
            {
                return;
            }
            var exceptionContext = new ExceptionContext(requestContext, remoteException);
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
            if (requestContext.Packet.fromClient == false)
            {
                FastWebSocketCommon.SetApiActionTaskResult(requestContext, this.taskSetActionTable);
                return;
            }

            var action = this.GetApiAction(requestContext);
            if (action == null)
            {
                return;
            }

            var actionContext = new ActionContext(requestContext, action);
            var fastApiService = this.GetFastApiService(actionContext);
            if (fastApiService == null)
            {
                return;
            }

            // 执行Api行为           
            fastApiService.Execute(actionContext);

            // 释放资源
            DependencyResolver.Current.TerminateService(fastApiService);
        }

        /// <summary>
        /// 获取Api行为
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        private ApiAction GetApiAction(RequestContext requestContext)
        {
            var action = this.apiActionList.TryGet(requestContext.Packet.api);
            if (action != null)
            {
                return action;
            }

            var exception = new ApiNotExistException(requestContext.Packet.api);
            var exceptionContext = new ExceptionContext(requestContext, exception);

            FastWebSocketCommon.SetRemoteException(this.JsonSerializer, exceptionContext);
            this.ExecExceptionFilters(exceptionContext);

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exception;
            }

            return null;
        }

        /// <summary>
        /// 获取ApiService实例
        /// </summary>
        /// <param name="actionContext">Api行为上下文</param>
        /// <returns></returns>
        private IFastApiService GetFastApiService(ActionContext actionContext)
        {
            // 获取服务实例
            var fastApiService = (IFastApiService)DependencyResolver.Current.GetService(actionContext.Action.DeclaringService);
            if (fastApiService != null)
            {
                return fastApiService;
            }
            var exception = new Exception(string.Format("无法获取类型{0}的实例", actionContext.Action.DeclaringService));
            var exceptionContext = new ExceptionContext(actionContext, exception);

            FastWebSocketCommon.SetRemoteException(this.JsonSerializer, exceptionContext);
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
                this.apiActionList = null;

                this.taskSetActionTable.Clear();
                this.taskSetActionTable = null;

                this.packetIdProvider = null;
                this.JsonSerializer = null;
                this.FilterAttributeProvider = null;
            }
        }
        #endregion
    }
}
