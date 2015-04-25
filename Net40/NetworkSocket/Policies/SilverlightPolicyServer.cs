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
    public sealed class SilverlightPolicyServer : TcpServerBase<SessionBase>
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
        /// <param name="session">会话对象</param>
        /// <param name="buffer">数据</param>      
        protected override void OnReceive(SessionBase session, ReceiveBuffer buffer)
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
            var byteRange = new ByteRange(bytes);

            try
            {
                session.Send(byteRange);
            }
            finally
            {
                session.Close();
            }
        }

        /// <summary>
        /// 创建新的会话对象
        /// </summary>
        /// <returns></returns>
        protected override SessionBase OnCreateSession()
        {
            return new SessionBase();
        }
    }
}
