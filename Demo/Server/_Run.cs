using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Collections.Concurrent;
using NetworkSocket.Policies;
using NetworkSocket;
using System.IO;

namespace Server
{
    class _Run
    {
        static void Main(string[] args)
        {

            Console.Title = "综合服务器";

            // silverlight安全策略服务
            var silverlight = new SilverlightPolicyServer();
            var owner = TcpTable.Snapshot().FirstOrDefault(item => item.Port == silverlight.Port);
            if (owner != null)
            {
                owner.Kill();
            }
            silverlight.StartListen();
            Console.WriteLine("Sliverlight策略服务启动：" + silverlight.LocalEndPoint);

            // fastserver服务
            var fastServer = new FastServer();
            fastServer.StartListen(4502);

            var demoPath = Environment.CurrentDirectory;
            while (demoPath.Contains("Server"))
            {
                demoPath = Path.GetDirectoryName(demoPath);
            }
            fastServer.ToProxyCode().WriteToFile(Path.Combine(demoPath, "ClientApp\\FastServerProxyBase.cs"));
            Console.WriteLine("FastServer服务启动：" + fastServer.LocalEndPoint);



            Console.WriteLine();
            Console.WriteLine("所有服务启动完成....");
            Console.WriteLine("请运行ClientApp或SilverlightApp或WebSocket.html进行测试...");
            Console.WriteLine();

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
