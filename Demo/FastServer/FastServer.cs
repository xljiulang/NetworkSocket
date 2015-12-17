using FastModels;
using FastServer.Services;
using NetworkSocket;
using NetworkSocket.Fast;
using NetworkSocket.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastServer
{
    /// <summary>
    /// MyFastServer服务 
    /// </summary>
    public class FastServer : FastTcpServer
    {
        public FastServer()
        {
            // 这里也可以在Model上打Attribute验证规则
            // 而FluentApi配置规则可以实现对Model无入侵
            Model.Fluent<UserInfo>()
                .Required(item => item.Account, "账号不能为空")
                .Length(item => item.Account, 10, 5, "账号为{0}到{1}个字符")
                .Required(item => item.Password, "密码不能为空")
                .Length(item => item.Password, 12, 6, "密码为{0}到{1}个字符");
        }

        /// <summary>
        /// 接收到会话连接
        /// </summary>
        /// <param name="session">会话</param>
        protected override void OnConnect(FastSession session)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), session, "Connect", "ConnectCount(" + this.AllSessions.Count() + ")");
            Console.WriteLine(log);
        }

        /// <summary>
        /// 接收到会话断开连接
        /// </summary>
        /// <param name="session">会话</param>
        protected override void OnDisconnect(FastSession session)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), session, "Disconnect", "ConnectCount(" + this.AllSessions.Count() + ")");
            Console.WriteLine(log);
        }
    }
}
