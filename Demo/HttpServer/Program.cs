using HttpServer.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var http = new NetworkSocket.Http.HttpServer();
            http.GlobalFilter.Add(new ExceptionFilterAttribute());
            http.BindController(typeof(Program).Assembly);
            http.StartListen(7777);

            if (Directory.Exists("js") == false)
            {
                Directory.SetCurrentDirectory("../../");
            }

            Console.WriteLine("http服务启动成功");
            Process.Start("http://localhost:7777/power/index");
            Console.ReadLine();
        }
    }
}
