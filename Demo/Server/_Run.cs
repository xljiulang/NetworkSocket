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

namespace Server
{
    class _Run
    {
        static void Main(string[] args)
        {
            Console.Title = "FastServer";

            var fastServer = new FastServer();
            fastServer.BindService();
            fastServer.StartListen(4502);

            #region 依赖注入断言
            var n1 = fastServer.Service<NotifyService>();
            var n2 = fastServer.Service<NotifyService>();
            Debug.Assert(object.ReferenceEquals(n1, n2));

            var s1 = fastServer.Service<SystemService>();
            var s2 = fastServer.Service<SystemService>();
            Debug.Assert(object.ReferenceEquals(s1, s2) == false);
            #endregion

            Console.WriteLine("FastServer服务启动");
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
