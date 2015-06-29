using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Fast;

namespace Autofac.NetworkSocket
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class FastTcpServerExtend
    {
        /// <summary>
        /// 依赖注入注册操作
        /// </summary>
        /// <param name="fastTcpServer">服务器</param>
        public static void RegisterDependencyResolver(this IFastTcpServer fastTcpServer, Action<ContainerBuilder> builderAction)
        {
            var builder = new ContainerBuilder();
            if (builderAction != null)
            {
                builderAction.Invoke(builder);
            }
            var container = builder.Build();
            fastTcpServer.DependencyResolver = new AutofacDependencyResolver(container);
        }

        /// <summary>
        /// 依赖注入注册过滤器
        /// </summary>
        /// <param name="fastTcpServer">服务器</param>
        public static void RegisterFilters(this IFastTcpServer fastTcpServer)
        {
            var dependencyResolver = fastTcpServer.DependencyResolver as AutofacDependencyResolver;
            if (dependencyResolver == null)
            {
                throw new Exception("请先调用RegisterDependencyResolver");
            }
            fastTcpServer.FilterAttributeProvider = new AutofacFilterAttributeProvider(dependencyResolver);
        }
    }
}
