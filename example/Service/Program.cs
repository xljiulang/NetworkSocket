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

            Model.Fluent<UserInfo>()
               .Required(item => item.Account, "账号不能为空")
               .Length(item => item.Account, 10, 5, "账号为{0}到{1}个字符")
               .Required(item => item.Password, "密码不能为空")
               .Length(item => item.Password, 12, 6, "密码为{0}到{1}个字符");

            // 需要先将openssl.pfx安装为信任，否则浏览器和客户端不信任
            var cert = new X509Certificate2("cert\\openssl.pfx", "123456");

            var listener = new TcpListener();
            listener.Use<HttpMiddleware>().GlobalFilters.Add(new HttpGlobalFilter());
            listener.Use<JsonWebSocketMiddleware>().GlobalFilters.Add(new WebSockeGlobalFilter());
            listener.Use<FastMiddleware>().GlobalFilters.Add(new FastGlobalFilter());
            listener.Use<FlexPolicyMiddleware>();

            listener.UsePlug<WebSocketPlug>();

            listener.UseSSL(cert);

            listener.Start(443);
            Process.Start("https://localhost:443");

            Console.Read();
            listener.Dispose();
        }
    }
}
