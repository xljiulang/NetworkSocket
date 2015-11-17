using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;
using NetworkSocket.WebSocket.Fast;

namespace Autofac.NetworkSocket
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class ServerExtend
    {
        /// <summary>
        /// 使用Autofac作依赖解析
        /// 并为相关类型进行注册
        /// </summary>
        /// <param name="fastTcpServer">服务器</param>
        public static void SetAutofacDependencyResolver(this IFastTcpServer fastTcpServer, Action<ContainerBuilder> builderAction)
        {
            var builder = new ContainerBuilder();
            if (builderAction != null)
            {
                builderAction.Invoke(builder);
            }
            var container = builder.Build();
            fastTcpServer.DependencyResolver = new DependencyResolver(container);
        } 


        /// <summary>
        /// 使用Autofac作过滤器依赖解析
        /// 使过滤器支持属性依赖注入功能
        /// </summary>
        /// <param name="fastTcpServer">服务器</param>
        public static void SetAutofacFilterAttributeProvider(this IFastTcpServer fastTcpServer)
        {
            var dependencyResolver = fastTcpServer.DependencyResolver as DependencyResolver;
            if (dependencyResolver == null)
            {
                throw new Exception("Autofac不是服务的DependencyResolver，请先调用SetAutofacDependencyResolver");
            }
            fastTcpServer.FilterAttributeProvider = new FastTcpFilterAttributeProvider(dependencyResolver);
        }

        /// <summary>
        /// 使用Autofac作依赖解析
        /// 并为相关类型进行注册
        /// </summary>
        /// <param name="fastWebSocketServer">服务器</param>
        public static void SetAutofacDependencyResolver(this IFastWebSocketServer fastWebSocketServer, Action<ContainerBuilder> builderAction)
        {
            var builder = new ContainerBuilder();
            if (builderAction != null)
            {
                builderAction.Invoke(builder);
            }
            var container = builder.Build();
            fastWebSocketServer.DependencyResolver = new DependencyResolver(container);
        }

        /// <summary>
        /// 使用Autofac作过滤器依赖解析
        /// 使过滤器支持属性依赖注入功能
        /// </summary>
        /// <param name="fastWebSocketServer">服务器</param>
        public static void SetAutofacFilterAttributeProvider(this IFastWebSocketServer fastWebSocketServer)
        {
            var dependencyResolver = fastWebSocketServer.DependencyResolver as DependencyResolver;
            if (dependencyResolver == null)
            {
                throw new Exception("Autofac不是服务的DependencyResolver，请先调用SetAutofacDependencyResolver");
            }
            fastWebSocketServer.FilterAttributeProvider = new FastWebSocketFilterAttributeProvider(dependencyResolver);
        }
    }
}
