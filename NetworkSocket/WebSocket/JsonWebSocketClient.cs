using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示JsonWebsocket协议客户端
    /// </summary>
    public class JsonWebSocketClient : WebSocketClient
    {
        /// <summary>
        /// 所有Api行为
        /// </summary>
        private ApiActionTable apiActionTable;

        /// <summary>
        /// 数据包id提供者
        /// </summary>
        private PacketIdProvider packetIdProvider;

        /// <summary>
        /// 任务行为表
        /// </summary>
        private TaskSetterTable<long> taskSetterTable;

        /// <summary>
        /// 获取或设置序列化工具       
        /// </summary>
        public IDynamicJsonSerializer JsonSerializer { get; set; }

        /// <summary>
        /// 获取或设置请求等待超时时间(毫秒) 
        /// 默认30秒
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TimeSpan TimeOut { get; set; }

        /// <summary>
        /// JsonWebsocket协议客户端
        /// </summary>
        public JsonWebSocketClient()
        {
            this.Init();
        }

        /// <summary>
        /// SSL支持的JsonWebsocket协议客户端
        /// </summary>
        /// <param name="targetHost">目标主机</param>
        /// <exception cref="ArgumentNullException"></exception>
        public JsonWebSocketClient(string targetHost)
            : base(targetHost)
        {
            this.Init();
        }

        /// <summary>
        /// SSL支持的JsonWebsocket协议客户端
        /// </summary>  
        /// <param name="targetHost">目标主机</param>
        /// <param name="certificateValidationCallback">远程证书验证回调</param>
        /// <exception cref="ArgumentNullException"></exception>
        public JsonWebSocketClient(string targetHost, RemoteCertificateValidationCallback certificateValidationCallback)
            : base(targetHost, certificateValidationCallback)
        {
            this.Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            this.apiActionTable = new ApiActionTable(this.FindApiActions());
            this.packetIdProvider = new PacketIdProvider();
            this.taskSetterTable = new TaskSetterTable<long>();
            this.TimeOut = TimeSpan.FromSeconds(30);
            this.JsonSerializer = new DefaultDynamicJsonSerializer();
        }

        /// <summary>
        /// 获取服务类型的Api行为
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        private IEnumerable<ApiAction> FindApiActions()
        {
            return this.GetType()
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Where(item => Attribute.IsDefined(item, typeof(ApiAttribute)))
                .Select(method => new ApiAction(method));
        }


        /// <summary>
        /// 调用远程端实现的服务方法        
        /// </summary>       
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param>  
        /// <exception cref="SocketException"></exception>      
        /// <exception cref="SerializerException"></exception>       
        public void InvokeApi(string api, params object[] parameters)
        {
            var packet = new JsonPacket
            {
                api = api,
                id = this.packetIdProvider.NewId(),
                state = true,
                fromClient = true,
                body = parameters
            };
            var packetJson = this.JsonSerializer.Serialize(packet);
            this.SendText(packetJson);
        }

        /// <summary>
        /// 调用客户端实现的服务方法     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>        
        /// <param name="api">api</param>
        /// <param name="parameters">参数</param> 
        /// <exception cref="SocketException"></exception>         
        /// <exception cref="SerializerException"></exception>
        /// <returns>远程数据任务</returns>  
        public Task<T> InvokeApi<T>(string api, params object[] parameters)
        {
            var packet = new JsonPacket
            {
                api = api,
                id = this.packetIdProvider.NewId(),
                state = true,
                fromClient = true,
                body = parameters
            };

            // 登记TaskSetAction             
            var task = this.taskSetterTable.Create<T>(packet.id, this.TimeOut);
            var packetJson = this.JsonSerializer.Serialize(packet);
            this.SendText(packetJson);
            return task;
        }


        /// <summary>
        /// 收到文本内容
        /// </summary>
        /// <param name="frame">数据帧</param>
        protected sealed override async void OnText(FrameRequest frame)
        {
            var text = Encoding.UTF8.GetString(frame.Content);
            var package = this.TryGetJsonPacket(text);
            if (package == null)
            {
                return;
            }

            if (package.state == false)
            {
                this.ProcessRemoteException(package);
            }
            else
            {
                await this.ProcessRequestAsync(package);
            }
        }

        /// <summary>
        /// 处理远返回的程异常
        /// </summary>
        /// <param name="package">数据包</param>     
        private void ProcessRemoteException(JsonPacket package)
        {
            var taskSetter = this.taskSetterTable.Take(package.id);
            if (taskSetter == null)
            {
                return;
            }

            var body = package.body;
            var message = body == null ? null : body.ToString();
            var exception = new RemoteException(message);
            taskSetter.SetException(exception);
        }

        /// <summary>
        /// 处理正常的数据请求
        /// </summary>
        /// <param name="package">数据包</param>  
        /// <returns></returns>
        private async Task ProcessRequestAsync(JsonPacket package)
        {
            if (package.fromClient == true)
            {
                this.SetApiActionTaskResult(package);
            }
            else
            {
                await this.TryExecuteRequestAsync(package);
            }
        }

        /// <summary>
        /// 执行请求
        /// </summary>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        private async Task TryExecuteRequestAsync(JsonPacket package)
        {
            try
            {
                var action = this.GetApiAction(package);
                var parameters = this.GetAndUpdateParameterValues(action, package);
                var result = await action.ExecuteAsync(this, parameters);

                if (action.IsVoidReturn == false && this.IsConnected)
                {
                    package.body = result;
                    this.TrySendPackage(package);
                }
            }
            catch (Exception ex)
            {
                package.state = false;
                package.body = ex.Message;
                this.TrySendPackage(package);
                this.OnException(ex);
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="package">数据包</param>
        /// <returns></returns>
        private bool TrySendPackage(JsonPacket package)
        {
            try
            {
                var packetJson = this.JsonSerializer.Serialize(package);
                return this.SendText(packetJson) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 获取Api行为
        /// </summary>
        /// <param name="package">数据包</param>
        /// <exception cref="ApiNotExistException"></exception>
        /// <returns></returns>
        private ApiAction GetApiAction(JsonPacket package)
        {
            var action = this.apiActionTable.TryGetAndClone(package.api);
            if (action == null)
            {
                throw new ApiNotExistException(package.api);
            }
            else
            {
                return action;
            }
        }


        /// <summary>
        /// 设置Api行为的参数值
        /// </summary> 
        /// <param name="action">api行为</param>        
        /// <param name="package">数据包</param>
        /// <exception cref="ArgumentException"></exception>   
        /// <returns></returns>
        private object[] GetAndUpdateParameterValues(ApiAction action, JsonPacket package)
        {
            var body = package.body as IList;
            if (body == null)
            {
                throw new ArgumentException("body参数必须为数组");
            }

            if (body.Count != action.Parameters.Length)
            {
                throw new ArgumentException("body参数数量不正确");
            }

            for (var i = 0; i < body.Count; i++)
            {
                var parameter = action.Parameters[i];
                parameter.Value = this.JsonSerializer.Convert(body[i], parameter.Type);
            }

            return action.Parameters.Select(p => p.Value).ToArray();
        }

        /// <summary>
        /// 设置Api行为返回的任务结果
        /// </summary>
        /// <param name="package">数据包</param>      
        /// <returns></returns>
        private bool SetApiActionTaskResult(JsonPacket package)
        {
            var taskSetter = this.taskSetterTable.Take(package.id);
            if (taskSetter == null)
            {
                return true;
            }

            try
            {
                var body = package.body;
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
        /// 尝试获取数据包
        /// </summary>     
        /// <param name="text">内容</param>        
        /// <returns></returns>
        private JsonPacket TryGetJsonPacket(string text)
        {
            try
            {
                var packet = this.JsonSerializer.Deserialize(text);
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
                this.OnException(ex);
                return null;
            }
        }

        /// <summary>
        /// 异常发生时
        /// </summary>
        /// <param name="ex">异常</param>
        protected void OnException(Exception ex)
        {
        }
    }
}
