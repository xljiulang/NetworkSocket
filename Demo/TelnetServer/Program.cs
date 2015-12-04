using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TelnetServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = typeof(Program).Namespace;
            var server = new TelnetListener();
            server.StartListen(2222);

            Console.WriteLine("请在cmd键入telnet 127.0.0.1 2222");
            Console.ReadLine();
        }
    }
}
