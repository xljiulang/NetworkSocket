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
    public sealed class FlexPolicyServer : TcpServerBase<SessionBase>
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
        /// <param name="session">会话对象</param>
        /// <param name="buffer">数据</param>      
        protected override void OnReceive(SessionBase session, ReceiveBuffer buffer)
        {
            var xml = "<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"*\"/></cross-domain-policy>\0";
            var bytes = Encoding.UTF8.GetBytes(xml.ToCharArray()); // 需要把字符串转为Char[]
            var byteRange = new ByteRange(bytes);

            try
            {
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
        /// 创建新的会话对象
        /// </summary>
        /// <returns></returns>
        protected override SessionBase OnCreateSession()
        {
            return new SessionBase();
        }
    }
}
