using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;
using NetworkSocket.Core;
using NetworkSocket.Http;
using NetworkSocket.WebSocket;

namespace Autofac.NetworkSocket
{
    /// <summary>
    /// 服务的Autofac扩展
    /// </summary>
    public static class AutofacServerExtend
    {
        /// <summary>
        /// 使用Autofac作依赖解析
        /// 并为相关类型进行注册
        /// </summary>
        /// <param name="server">服务器</param>
        public static void SetAutofacDependencyResolver(this IDependencyResolverSupportable server, Action<ContainerBuilder> builderAction)
        {
            var builder = new ContainerBuilder();
            if (builderAction != null)
            {
                builderAction.Invoke(builder);
            }
            var container = builder.Build();
            server.DependencyResolver = new AutofacDependencyResolver(container);
        }


        /// <summary>
        /// 使用Autofac作过滤器依赖解析
        /// 使过滤器支持属性依赖注入功能
        /// </summary>
        /// <param name="server">服务器</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static void SetAutofacFilterAttributeProvider(this IFilterSupportable server)
        {
            var dependencyServer = server as IDependencyResolverSupportable;
            if (dependencyServer == null)
            {
                throw new ArgumentException("服务不支持依赖注入 ..");
            }
            var dependencyResolver = dependencyServer.DependencyResolver as AutofacDependencyResolver;
            if (dependencyResolver == null)
            {
                throw new Exception("Autofac不是服务的DependencyResolver，请先调用SetAutofacDependencyResolver");
            }
            server.FilterAttributeProvider = new AutofacFilterAttributeProvider(dependencyResolver);
        }
    }
}
