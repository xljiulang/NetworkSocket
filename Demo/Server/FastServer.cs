using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkSocket.Fast;
using NetworkSocket;
using Models;
using Autofac;
using System.Reflection;
using Server.Interfaces;
using Server.Services;
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
                .Where(type => (typeof(IFastService).IsAssignableFrom(type)))
                .PropertiesAutowired();

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
        protected override void OnConnect(IClient<FastPacket> client)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), client, "Connect", "ConnectCount(" + this.AliveClients.Count + ")");
            Console.WriteLine(log);
        }

        /// <summary>
        /// 接收到客户端断开连接
        /// </summary>
        /// <param name="client">客户端</param>
        protected override void OnDisconnect(IClient<FastPacket> client)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), client, "Disconnect", "ConnectCount(" + this.AliveClients.Count + ")");
            Console.WriteLine(log);
        }
    }
}
