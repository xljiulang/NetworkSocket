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
using System.Reflection;
using NetworkSocket.Fast;
using Server.Services;
using Server.Filters;

namespace Server
{
    class _Run
    {
        static void Main(string[] args)
        {

            GlobalFilters.Add(new ExceptionFilterAttribute());

            var fastServer = new FastServer();
            fastServer.BindService();
            fastServer.RegisterResolver();
            fastServer.StartListen(4502);

            Console.Title = "FastServer V" + fastServer.GetService<SystemService>().GetVersion();
            Console.WriteLine("服务已启动，端口：" + fastServer.LocalEndPoint.Port);
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
