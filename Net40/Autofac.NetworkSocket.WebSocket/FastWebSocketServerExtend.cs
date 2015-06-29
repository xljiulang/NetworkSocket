using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.NetworkSocket.WebSocket
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class FastWebSocketServerExtend
    {
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
            fastWebSocketServer.DependencyResolver = new AutofacDependencyResolver(container);
        }

        /// <summary>
        /// 使用Autofac作过滤器依赖解析
        /// 使过滤器支持属性依赖注入功能
        /// </summary>
        /// <param name="fastWebSocketServer">服务器</param>
        public static void SetAutofacFilterAttributeProvider(this IFastWebSocketServer fastWebSocketServer)
        {
            var dependencyResolver = fastWebSocketServer.DependencyResolver as AutofacDependencyResolver;
            if (dependencyResolver == null)
            {
                throw new Exception("Autofac不是服务的DependencyResolver，请先调用SetAutofacDependencyResolver");
            }
            fastWebSocketServer.FilterAttributeProvider = new AutofacFilterAttributeProvider(dependencyResolver);
        }
    }
}
