using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// Siverlight通讯策略服务   
    /// </summary>
    public class SilverlightPolicyServer : TcpServerBase<SessionBase>
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
        /// 创建新的会话对象
        /// </summary>
        /// <returns></returns>
        protected sealed override SessionBase OnCreateSession()
        {
            return new SessionBase();
        }

        /// <summary>
        /// 接收到策略请求
        /// </summary>
        /// <param name="session">会话对象</param>
        /// <param name="buffer">数据</param>      
        protected sealed override void OnReceive(SessionBase session, ReceiveStream buffer)
        {
            var input = buffer.ReadString(buffer.Length, Encoding.UTF8);
            var policyXml = this.OnGetPolicyXml(input);

            try
            {
                var bytes = Encoding.UTF8.GetBytes(policyXml);
                var byteRange = new ByteRange(bytes);
                session.Send(byteRange);
            }
            catch (Exception)
            {
            }
            finally
            {
                session.Close();
            }
        }

        /// <summary>
        /// 请求获取策略xml
        /// </summary>
        /// <param name="input">请求内容</param>
        /// <returns></returns>
        protected virtual string OnGetPolicyXml(string input)
        {
            return new StringBuilder()
                .AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>")
                .AppendLine("<access-policy>")
                .AppendLine("<cross-domain-access>")
                .AppendLine("<policy>")
                .AppendLine("<allow-from>")
                .AppendLine("<domain uri=\"*\"/>")
                .AppendLine("</allow-from>")
                .AppendLine("<grant-to>")
                .AppendLine("<socket-resource port=\"4502-4534\" protocol=\"tcp\"/>")
                .AppendLine("</grant-to>")
                .AppendLine("</policy>")
                .AppendLine("</cross-domain-access>")
                .AppendLine("</access-policy>")
                .ToString();
        }
    }
}
