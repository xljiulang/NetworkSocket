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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("请将cert\\openssl.pfx安装为信任，否则浏览器和客户端不信任");
            Console.ForegroundColor = ConsoleColor.Gray;
            var cert = new X509Certificate2("cert\\openssl.pfx", "123456");

            var listener = new TcpListener();

            Console.WriteLine("Use<HttpMiddleware>");
            listener.Use<HttpMiddleware>().GlobalFilters.Add(new HttpGlobalFilter());

            Console.WriteLine("Use<JsonWebSocketMiddleware>");
            listener.Use<JsonWebSocketMiddleware>().GlobalFilters.Add(new WebSockeGlobalFilter());

            Console.WriteLine("Use<FastMiddleware>");
            listener.Use<FastMiddleware>().GlobalFilters.Add(new FastGlobalFilter());

            Console.WriteLine("UsePlug<WebSocketPlug>");
            listener.UsePlug<WebSocketPlug>();

            Console.WriteLine("UseSSL<cert\\openssl.pfx>");
            listener.UseSSL(cert);

            Console.WriteLine("Listening port 443");
            listener.Start(443);
            Process.Start("https://localhost:443");

            Console.Read();
            listener.Dispose();
        }
    }
}
