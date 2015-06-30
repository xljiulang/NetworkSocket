using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Fast会话对象
    /// 不可继承
    /// </summary>
    public sealed class FastSession : SessionBase, IFastSession
    {
        /// <summary>
        /// 获取服务器实例
        /// </summary>
        internal FastTcpServer Server { get; private set; } 

        /// <summary>
        /// 服务器的客户端对象
        /// </summary>
        /// <param name="server">服务器实例</param>
        internal FastSession(FastTcpServer server )
        {
            this.Server = server;
        }

        /// <summary>
        /// 调用远程端实现的Api        
        /// </summary>        
        /// <param name="api">数据包Api名</param>
        /// <param name="parameters">参数列表</param>      
        /// <exception cref="SocketException"></exception>     
        /// <exception cref="SerializerException"></exception>   
        /// <exception cref="ProtocolException"></exception>
        public void InvokeApi(string api, params object[] parameters)
        {
            var id = this.Server.PacketIdProvider.NewId();
            var packet = new FastPacket(api, id, false);
            packet.SetBodyParameters(this.Server.Serializer, parameters);
            this.Send(packet.ToByteRange());
        }

        /// <summary>
        /// 调用远程端实现的Api      
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>        
        /// <param name="api">数据包Api名</param>
        /// <param name="parameters">参数</param>       
        /// <exception cref="SocketException"></exception>      
        /// <exception cref="SerializerException"></exception>
        /// <exception cref="ProtocolException"></exception>
        /// <returns>远程数据任务</returns>         
        public Task<T> InvokeApi<T>(string api, params object[] parameters)
        {
            var id = this.Server.PacketIdProvider.NewId();
            var packet = new FastPacket(api, id, false);
            packet.SetBodyParameters(this.Server.Serializer, parameters);
            return FastTcpCommon.InvokeApi<T>(this, this.Server.TaskSetActionTable, this.Server.Serializer, packet);
        }
    }
}
