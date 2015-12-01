using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var http = new NetworkSocket.Http.HttpServer();
            http.GlobalFilter.Add(new Filters.ExceptionFilterAttribute());
            http.RegisterControllers(typeof(Program).Assembly);
            http.StartListen(7777);
            Console.WriteLine("http服务启动成功");            
            Process.Start("http://localhost:7777/api/token/test?x=1&y=2");                             
            Console.ReadLine();
        }
    }
}
