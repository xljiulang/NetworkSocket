using NetworkSocket.Fast.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 为实现IDependencyResolver接口的依赖关系解析程序提供一个注册点
    /// </summary>
    public static class DependencyResolver
    {
        /// <summary>
        /// 获取依赖关系解析程序的实现
        /// </summary>
        public static IDependencyResolver Current { get; private set; }

        /// <summary>
        /// 静态构造器
        /// </summary>
        static DependencyResolver()
        {
            DependencyResolver.Current = new DefaultDependencyResolver();
        }

        /// <summary>
        /// 使用指定的依赖关系解析程序接口，为依赖关系解析程序提供一个注册点
        /// </summary>
        /// <param name="resolver">依赖关系解析程序</param>
        public static void SetResolver(IDependencyResolver resolver)
        {
            DependencyResolver.Current = resolver;
        }
    }
}
