using Models;
using NetworkSocket;
using NetworkSocket.Fast;
using NetworkSocket.Flex;
using NetworkSocket.Http;
using NetworkSocket.Validation;
using NetworkSocket.WebSocket;
using Service.Fast;
using Service.Http;
using Service.Websocket;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "通讯服务器";
            if (Directory.Exists("js") == false)
            {
                Directory.SetCurrentDirectory("../../");
            }

            var listener = new TcpListener();
            ConfigMiddleware(listener);
            ConfigValidation();
            listener.Start(1212);

            Process.Start("http://localhost:1212/home/index");
            Console.ReadLine();
        }

        /// <summary>
        /// 配置模型验证
        /// </summary>
        public static void ConfigValidation()
        {
            Model.Fluent<UserInfo>()
                .Required(item => item.Account, "账号不能为空")
                .Length(item => item.Account, 10, 5, "账号为{0}到{1}个字符")
                .Required(item => item.Password, "密码不能为空")
                .Length(item => item.Password, 12, 6, "密码为{0}到{1}个字符");
        }

        /// <summary>
        /// 配置中间件
        /// </summary>
        /// <param name="listener"></param>
        public static void ConfigMiddleware(TcpListener listener)
        {
            listener.Use<HttpMiddleware>().GlobalFilters.Add(new HttpGlobalFilter());
            listener.Use<JsonWebSocketMiddleware>().GlobalFilters.Add(new WebSockeGlobalFilter());
            listener.Use<FastMiddleware>().GlobalFilters.Add(new FastGlobalFilter());
            listener.Use<FlexPolicyMiddleware>();
            listener.Events.OnDisconnected += Events_OnDisconnected;
        }


        /// <summary>
        /// 会话断开连接时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="context"></param>
        static void Events_OnDisconnected(object sender, IContenxt context)
        {
            if (context.Session.Protocol != Protocol.WebSocket)
            {
                return;
            }

            var name = context.Session.Tag.Get("name");
            if (name.IsNull == true)
            {
                return;
            }

            var webSocketSessions = context
                .AllSessions
                .FilterWrappers<JsonWebSocketSession>();

            var members = webSocketSessions
                .Select(item => item.Tag.Get("name").AsString())
                .Where(item => item != null)
                .ToArray();

            // 推送成员下线通知
            foreach (var item in webSocketSessions)
            {
                item.InvokeApi("OnMemberChange", 0, name, members);
            }
        }
    }
}
