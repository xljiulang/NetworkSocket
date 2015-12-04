using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// Flex通讯策略服务   
    /// </summary>
    public class FlexPolicyServer : TcpServerBase<SessionBase>
    {
        /// <summary>
        /// 获取策略服务端口
        /// 843
        /// </summary>
        public virtual int Port
        {
            get
            {
                return 843;
            }
        }

        /// <summary>
        /// 启动策略服务             
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
            var policyXml = this.GeneratePolicyXml(input);

            if (policyXml != null)
            {
                var bytes = Encoding.UTF8.GetBytes(policyXml.ToCharArray());
                try
                {
                    session.Send(bytes);
                }
                catch (Exception) { }
            }
            session.Close();
        }

        /// <summary>
        /// 生成策略xml
        /// 返回null则不发送策略文件 
        /// </summary>
        /// <param name="input">请求内容</param>
        /// <returns></returns>
        protected virtual string GeneratePolicyXml(string input)
        {
            return new StringBuilder()
                .AppendLine("<cross-domain-policy>")
                .AppendLine("<allow-access-from domain=\"*\" to-ports=\"*\"/>")
                .AppendLine("</cross-domain-policy>\0")
                .ToString();
        }
    }
}
