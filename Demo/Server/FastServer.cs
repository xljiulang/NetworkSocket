using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkSocket.Fast;
using NetworkSocket;
using Models;
using System.Reflection;
using Server.Interfaces;
using Server.Services;
using Server.Database;
using Autofac;
using Autofac.NetworkSocket;

namespace Server
{
    /// <summary>
    /// FastServer服务 
    /// </summary>
    public class FastServer : FastTcpServer
    {
        /// <summary>
        /// 注册依赖注入
        /// </summary>
        public void RegisterResolver()
        {           
            this.SetAutofacDependencyResolver((builder) =>
            {
                // 注册服务            
                builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                    .Where(type => (typeof(IFastApiService).IsAssignableFrom(type)))
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
            });

            // 给过滤器添加属性注入
            this.SetAutofacFilterAttributeProvider();          
        }


        /// <summary>
        /// 接收到会话连接
        /// </summary>
        /// <param name="session">会话</param>
        protected override void OnConnect(FastSession session)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), session, "Connect", "ConnectCount(" + this.AllSessions.Count() + ")");
            Console.WriteLine(log);
        }

        /// <summary>
        /// 接收到会话断开连接
        /// </summary>
        /// <param name="session">会话</param>
        protected override void OnDisconnect(FastSession session)
        {
            var log = string.Format("Time:{0} Client:{1} Action:{2} Message:{3}", DateTime.Now.ToString("mm:ss"), session, "Disconnect", "ConnectCount(" + this.AllSessions.Count() + ")");
            Console.WriteLine(log);
        }

        protected override void OnException(object sender, Exception exception)
        {
            Console.WriteLine(exception.Message);
            base.OnException(sender, exception);
        }
    }
}
