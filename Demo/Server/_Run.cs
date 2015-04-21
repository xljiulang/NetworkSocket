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
using Models.Serializer;

namespace Server
{
    class _Run
    {
        static void Main(string[] args)
        {

            GlobalFilters.Add(new ExceptionFilterAttribute());

            var fastServer = new FastServer();
            fastServer.Serializer = new FastJsonSerializer();
            fastServer.BindService(fastServer.GetType().Assembly);
            fastServer.RegisterResolver();
            fastServer.StartListen(1350);

            Console.Title = "FastServer V" + new SystemService().GetVersion();
            Console.WriteLine("服务已启动，端口：" + fastServer.LocalEndPoint.Port);
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
