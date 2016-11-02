using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示JsonWebSocket的会话对象
    /// </summary>
    public class JsonWebSocketSession : IWrapper
    {
        /// <summary>
        /// 会话对象
        /// </summary>
        private readonly WebSocketSession session;

        /// <summary>
        /// 获取中间件实例
        /// </summary>
        internal JsonWebSocketMiddleware Middleware { get; private set; }

        /// <summary>
        /// 获取用户数据字典
        /// </summary>
        public ITag Tag
        {
            get
            {
                return this.session.Tag;
            }
        }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.session.RemoteEndPoint;
            }
        }

        /// <summary>
        /// 获取本机终结点
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                return this.session.LocalEndPoint;
            }
        }

        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.session.IsConnected;
            }
        }

        /// <summary>
        /// JsonWebSocket的客户端对象
        /// </summary>
        /// <param name="server">服务器实例</param>
        /// <param name="session">WebSocket会话</param>
        public JsonWebSocketSession(JsonWebSocketMiddleware server, WebSocketSession session)
        {
            this.Middleware = server;
            this.session = session;
        }

        /// <summary>
        /// 向客户端ping唯一的内容
        /// 并等待匹配的回复
        /// </summary>
        /// <param name="waitTime">最多等待时间，超时则结果false</param>
        /// <returns></returns>
        public Task<bool> PingAsync(TimeSpan waitTime)
        {
            return this.session.PingAsync(waitTime);
        }

        /// <summary>
        /// 正常关闭客户端
        /// </summary>       
        /// <param name="code">关闭码</param>
        public void Close(StatusCodes code)
        {
            this.session.Close(code);
        }

        /// <summary>
        /// 正常关闭客户端
        /// </summary>      
        /// <param name="code">关闭码</param>
        /// <param name="reason">原因</param>
        public void Close(StatusCodes code, string reason)
        {
            this.session.Close(code, reason);
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
                id = this.Middleware.PacketIdProvider.NewId(),
                state = true,
                fromClient = false,
                body = parameters
            };
            var packetJson = this.Middleware.JsonSerializer.Serialize(packet);
            this.session.SendText(packetJson);
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
                id = this.Middleware.PacketIdProvider.NewId(),
                state = true,
                fromClient = false,
                body = parameters
            };

            // 登记TaskSetAction             
            var task = this.Middleware.TaskSetterTable.Create<T>(packet.id, this.Middleware.TimeOut);
            var packetJson = this.Middleware.JsonSerializer.Serialize(packet);
            this.session.SendText(packetJson);
            return task;
        }


        /// <summary>
        /// 还原到包装前
        /// </summary>
        /// <returns></returns>
        public WebSocketSession UnWrap()
        {
            return this.session;
        }

        /// <summary>
        /// 还原到包装前
        /// </summary>
        /// <returns></returns>
        ISession IWrapper.UnWrap()
        {
            return this.session.UnWrap();
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.session.ToString();
        }
    }
}
