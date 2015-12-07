using NetworkSocket.Http;
using NetworkSocket.Policies;
using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WebsocketChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = typeof(Program).Namespace;

            // 用于提供静态html页面，也可以用IIS
            var http = new HttpServer();
            http.StartListen(82);
            Console.WriteLine("http服务启动成功 ..");

            // 当浏览器不支持websocket时，将由flash实现            
            var flexPolicyServer = new FlexPolicyServer();
            flexPolicyServer.StartListen();
            Console.WriteLine("flex策略服务启动成功 ..");

            // websocket服务
            var webSocket = new ChatWebSocketServer();
            webSocket.StartListen(83);
            Console.WriteLine("WebSocket服务启动成功 ..");


            // 设置进程的工作目录
            if (Directory.Exists("js") == false)
            {
                Directory.SetCurrentDirectory("../../");
            }
            Process.Start("http://localhost:82/index.html");
            Console.ReadLine();
        }
    }
}
