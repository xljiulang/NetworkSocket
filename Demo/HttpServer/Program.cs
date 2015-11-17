using System;
using System.Collections.Generic;
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
            Console.ReadLine();
        }
    }
}
