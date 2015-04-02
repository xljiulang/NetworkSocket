using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkSocket.Fast;
using NetworkSocket;
using NetworkSocket.Fast.Attributes;
using Models;
using Autofac;
using System.Reflection;
using Server.Interfaces;
using Server.Services;
using NetworkSocket.Fast.Filters;
using Server.Database;

namespace Server
{
    /// <summary>
    /// FastServer服务 
    /// </summary>
    public class FastServer : FastTcpServerBase
    {
        /// <summary>
        /// 注册依赖注入
        /// </summary>
        public void RegisterResolver()
        {
            var builder = new ContainerBuilder();

            // 注册服务            
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(type => (typeof(FastServiceBase).IsAssignableFrom(type)))
                .PropertiesAutowired();

            // 通知服务为单例
            builder.RegisterType<NotifyService>()
                .SingleInstance();

            // 注册DbContext           
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .Where(type => (typeof(IDbContext).IsAssignableFrom(type)))
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

            // 注册Dao
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .Where(type => (typeof(IDao).IsAssignableFrom(type)))
               .PropertiesAutowired()
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

            // 注册日志
            builder.RegisterType<Loger>()
                .As<ILog>()
                .InstancePerLifetimeScope();

            var container = builder.Build();

            // 设置依赖关系解析程序
            DependencyResolver.SetResolver(new AutofacResolver(container));

            // 给过滤器添加属性注入
            this.FilterAttributeProvider = new AutofacFilterAttributeProvider();
        }

       

        /// <summary>
        /// 接收到客户端连接
        /// </summary>
        /// <param name="client">客户端</param>
        protected override void OnConnect(SocketAsync<FastPacket> client)
        {
            Console.WriteLine("客户端{0}连接进来，当前连接数为：{1}", client, this.AliveClients.Count);
        }

        /// <summary>
        /// 接收到客户端断开连接
        /// </summary>
        /// <param name="client">客户端</param>
        protected override void OnDisconnect(SocketAsync<FastPacket> client)
        {
            Console.WriteLine("客户端{0}断开连接，当前连接数为：{1}", client, this.AliveClients.Count);
        }


        public override void OnException(ExceptionContext filterContext)
        {
            Console.WriteLine(filterContext.Exception);
            // filterContext.ExceptionHandled = true;
        }   
    }
}
