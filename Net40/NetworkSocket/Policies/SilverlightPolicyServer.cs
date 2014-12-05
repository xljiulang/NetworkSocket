using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// Siverlight通讯策略服务
    /// </summary>
    public sealed class SilverlightPolicyServer : TcpServerBase<PolicyPacket>
    {
        /// <summary>
        /// 本地943端口
        /// </summary>
        public int Port
        {
            get
            {
                return 943;
            }
        }

        /// <summary>
        /// 启动策略服务
        /// 监听本地943端口
        /// </summary>
        public void StartListen()
        {
            this.StartListen(this.Port);
        }

        /// <summary>
        /// 接收到策略请求
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="recvBuilder">数据</param>
        /// <returns></returns>
        protected override PolicyPacket OnReceive(SocketAsync<PolicyPacket> client, ByteBuilder recvBuilder)
        {
            return PolicyPacket.GetPacket(recvBuilder);
        }

        /// <summary>
        /// 完成一次策略请求解析
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">请求的数据包</param>
        protected override void OnRecvComplete(SocketAsync<PolicyPacket> client, PolicyPacket packet)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            xml.AppendLine("<access-policy>");
            xml.AppendLine("<cross-domain-access>");
            xml.AppendLine("<policy>");
            xml.AppendLine("<allow-from>");
            xml.AppendLine("<domain uri=\"*\"/>");
            xml.AppendLine("</allow-from>");
            xml.AppendLine("<grant-to>");
            xml.AppendLine("<socket-resource port=\"4502-4534\" protocol=\"tcp\"/>");
            xml.AppendLine("</grant-to>");
            xml.AppendLine("</policy>");
            xml.AppendLine("</cross-domain-access>");
            xml.AppendLine("</access-policy>");

            packet.Bytes = Encoding.UTF8.GetBytes(xml.ToString());
            client.Send(packet);
            // 一定要关闭才生效
            this.CloseClient(client);
        }
    }
}
