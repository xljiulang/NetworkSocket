using Service.AppStart;
using NetworkSocket;
using NetworkSocket.Flex;
using System;
using System.Diagnostics;
using System.IO;
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
            Config.ConfigMiddleware(listener);
            Config.ConfigValidation();

            listener.Start(1212);

            Process.Start("http://localhost:1212/home/index");
            Console.ReadLine();
        }
    }
}
