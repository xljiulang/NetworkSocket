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

            var sslListener = new TcpListener();
            Config.ConfigMiddleware(sslListener);
            Config.ConfigValidation();
            sslListener.Start(1212);

            Process.Start("http://localhost:1212/home/index");
            Console.ReadLine();
        }
    }
}
