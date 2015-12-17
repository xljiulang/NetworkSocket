using HttpServer.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NetworkSocket.Http;

namespace HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = typeof(Program).Namespace;

            var http = new NetworkSocket.Http.HttpServer();
            http.GlobalFilters.Add(new ExceptionFilterAttribute());
            http.BindController(typeof(Program).Assembly);
            http.StartListen(7777);

            if (Directory.Exists("js") == false)
            {
                Directory.SetCurrentDirectory("../../");
            }

            Console.WriteLine("http服务启动成功 ..");
            Process.Start("http://localhost:7777/power/index");

            Console.WriteLine();
            Console.WriteLine("请按任意键向浏览器推送内容 ..");
            while (true)
            {
                foreach (var item in http.EventSessions)
                {
                    var httpEvent = new HttpEvent { Data = "更多信息，请查询Server Sent Events .." };
                    item.SendEvent(httpEvent);
                }
                Console.ReadLine();
            }
        }
    }
}
