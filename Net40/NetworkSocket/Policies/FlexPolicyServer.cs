using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// Flex通讯策略服务
    /// 不可继承
    /// </summary>
    public sealed class FlexPolicyServer : TcpServerBase<BinaryPacket, byte[]>
    {
        /// <summary>
        /// 本地843端口
        /// </summary>
        public int Port
        {
            get
            {
                return 843;
            }
        }

        /// <summary>
        /// 启动策略服务
        /// 监听本地843端口       
        /// </summary>
        /// <exception cref="SocketException"></exception>
        public void StartListen()
        {
            this.StartListen(this.Port);
        }

        /// <summary>
        /// 接收到策略请求
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="builder">数据</param>
        /// <returns></returns>
        protected override IEnumerable<byte[]> OnReceive(IClient<BinaryPacket> client, ByteBuilder builder)
        {
            if (builder.Length == 0)
            {
                yield break;
            }
            yield return builder.ToArrayThenClear();
        }

        /// <summary>
        /// 完成一次策略请求解析
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="tRecv">接收到的数据类型</param>
        protected override void OnRecvComplete(IClient<BinaryPacket> client, byte[] tRecv)
        {
            string xml = "<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"*\"/></cross-domain-policy>\0";
            // 需要把字符串转为Char[]
            var bytes = Encoding.UTF8.GetBytes(xml.ToCharArray());
            client.TrySend(bytes);
        }
    }
}
