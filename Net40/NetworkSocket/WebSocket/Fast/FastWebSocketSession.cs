using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// FastWebSocket的会话对象
    /// 不可继承
    /// </summary>
    public sealed class FastWebSocketSession : WebSocketSession, IFastWebSocketSession
    {
        /// <summary>
        /// 获取服务器实例
        /// </summary>
        internal FastWebSocketServer Server { get; private set; }

        /// <summary>
        /// FastWebSocket的客户端对象
        /// </summary>
        /// <param name="server">服务器实例</param>
        internal FastWebSocketSession(FastWebSocketServer server)
        {
            this.Server = server;
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
            var packet = new FastPacket
            {
                api = api,
                id = this.Server.PacketIdProvider.NewId(),
                state = true,
                fromClient = false,
                body = parameters
            };
            var packetJson = this.Server.JsonSerializer.Serialize(packet);
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
            var packet = new FastPacket
            {
                api = api,
                id = this.Server.PacketIdProvider.NewId(),
                state = true,
                fromClient = false,
                body = parameters
            };

            // 登记TaskSetAction       
            var taskSource = new TaskCompletionSource<T>();
            var taskSetAction = new TaskSetAction<T>(taskSource);
            this.Server.TaskSetActionTable.Add(packet.id, taskSetAction);

            var packetJson = this.Server.JsonSerializer.Serialize(packet);
            this.SendText(packetJson);
            return taskSource.Task;
        }
    }
}
