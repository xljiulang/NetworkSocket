using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// Siverlight通讯策略服务
    /// 不可继承
    /// </summary>
    public sealed class SilverlightPolicyServer : TcpServerBase<BinaryPacket, byte[]>
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
        /// <param name="rRecv">接收到的数据类型</param>
        protected override void OnRecvComplete(IClient<BinaryPacket> client, byte[] rRecv)
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

            var bytes = Encoding.UTF8.GetBytes(xml.ToString());
            client.TrySend(bytes);
            // 一定要关闭才生效
            client.Close();
        }
    }
}
