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
using System.Threading.Tasks;

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
            Program.ConfigUse(listener);
            Program.ConfigValidation();

            listener.Start(1212);
            Process.Start("http://localhost:1212");
            Console.ReadLine();
        }

        /// <summary>
        /// 配置协议与插件
        /// </summary>
        /// <param name="listener"></param>
        public static void ConfigUse(TcpListener listener)
        {
            listener.Use<HttpMiddleware>().GlobalFilters.Add(new HttpGlobalFilter());
            listener.Use<JsonWebSocketMiddleware>().GlobalFilters.Add(new WebSockeGlobalFilter());
            listener.Use<FastMiddleware>().GlobalFilters.Add(new FastGlobalFilter());
            listener.Use<FlexPolicyMiddleware>();

            listener.UsePlug<WebSocketPlug>();
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
        /// 查找服务器证书
        /// </summary>
        /// <returns></returns>
        private static X509Certificate2 FindCert()
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);

            var cert = store.Certificates
                .Cast<X509Certificate2>()
                .Where(item => item.IssuerName.Name == "CN=localhost")
                .FirstOrDefault();

            store.Close();
            return cert;
        }

    }
}
