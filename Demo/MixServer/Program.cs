using MixServer.AppStart;
using NetworkSocket;
using System;
using System.Diagnostics;
using System.IO;
namespace MixServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "混合协议通讯服务器";

            var listener = new TcpListener();
            Config.ConfigMiddleware(listener);
            Config.ConfigValidation();
            listener.Start(1212);

            if (Directory.Exists("js") == false)
            {
                Directory.SetCurrentDirectory("../../");
            }

            Process.Start("http://localhost:1212/home/index");
            Console.ReadLine();
        }
    }
}
