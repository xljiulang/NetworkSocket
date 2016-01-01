using MixServer.AppStart;
using NetworkSocket;
using NetworkSocket.Flex;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
namespace MixServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "混合协议通讯服务器";
            if (Directory.Exists("js") == false)
            {
                Directory.SetCurrentDirectory("../../");
            }

            var cer = X509Certificate.CreateFromCertFile("ssl.cer");
            Console.WriteLine("为了更好的演示效果");
            Console.WriteLine("请先将不受信任的ssl.cer证书安装到受信任的根证书颁发机构 ..");
            Console.WriteLine("当然正式服务环境还是需要购买证书的");

            // flex策略服务不支持SSL，所以不能与1212共用端口
            var flex = new TcpListener();
            flex.Use<FlexPolicyMiddleware>();
            flex.Start(843);

            var sslListener = new TcpListener(cer);
            Config.ConfigMiddleware(sslListener);
            Config.ConfigValidation();
            sslListener.Start(1212);

            Process.Start("https://localhost:1212/home/index");
            Console.ReadLine();
        }
    }
}
