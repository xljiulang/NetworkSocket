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
        #region 依赖注入
        /// <summary>
        /// Autofac依赖注入容器
        /// </summary>
        private IContainer container;

        /// <summary>
        /// Autofac生命范围
        /// </summary>
        [ThreadStatic]
        private static ILifetimeScope LiftTimeScope;

        /// <summary>
        /// FastServer
        /// </summary>
        public FastServer()
        {
            this.RegisterResolver();
        }

        /// <summary>
        /// 依赖转换控制
        /// </summary>
        private void RegisterResolver()
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

            this.container = builder.Build();
        }

        /// <summary>
        /// 使用Autofac获取服务实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        protected override FastServiceBase GetService(Type serviceType)
        {
            LiftTimeScope = this.container.BeginLifetimeScope();
            return LiftTimeScope.Resolve(serviceType) as FastServiceBase;
        }

        /// <summary>
        /// 使用Autofac管理服务生命周期
        /// </summary>
        /// <param name="service">服务实例</param>
        protected override void DisposeService(FastServiceBase service)
        {
            LiftTimeScope.Dispose();
        }

        /// <summary>
        /// 给过滤器添加属性注入
        /// </summary>
        /// <param name="action">服务行为</param>
        /// <returns></returns>
        protected override IEnumerable<IFilter> GetFilters(FastAction action)
        {
            return base.GetFilters(action).Select(filter => LiftTimeScope.InjectProperties(filter));
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : FastServiceBase
        {
            return this.container.Resolve<T>();
        }
        #endregion

        #region 消息处理
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

        public override void OnAuthorization(ActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
        }

        public override void OnExecuting(ActionContext actionContext)
        {
            base.OnExecuting(actionContext);
        }

        public override void OnExecuted(ActionContext actionContext)
        {
            base.OnExecuted(actionContext);
        }

        public override void OnException(ExceptionContext exceptionContext)
        {
            base.OnException(exceptionContext);
        }
        #endregion


    }
}
