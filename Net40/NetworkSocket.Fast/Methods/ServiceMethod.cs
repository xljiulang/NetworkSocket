using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NetworkSocket.Fast.Attributes;

namespace NetworkSocket.Fast.Methods
{
    /// <summary>
    /// 表示带Service特性的方法信息
    /// </summary>
    internal class ServiceMethod : FastMethod
    {
        /// <summary>
        /// 获取返回类型是否为非void
        /// </summary>
        public bool HasReturn { get; private set; }

        /// <summary>
        /// 获取方法的参数
        /// </summary>
        public ParameterInfo[] Parameters { get; private set; }

        /// <summary>
        /// 获取方法的参数类型
        /// </summary>
        public Type[] ParameterTypes { get; set; }

        /// <summary>
        /// 获取方法特性修饰
        /// </summary>
        public ServiceAttribute ServiceAttribute { get; private set; }

        /// <summary>
        /// 获取所有过滤器
        /// </summary>
        public FilterAttribute[] Filters { get; private set; }

        /// <summary>
        /// 获取声明该成员的类型
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                return this.Method.DeclaringType;
            }
        }

        /// <summary>
        /// 表示带Service特性的方法信息
        /// </summary>
        /// <param name="method">方法信息</param>
        public ServiceMethod(MethodInfo method)
            : base(method)
        {
            this.HasReturn = method.ReturnType.Equals(typeof(void)) == false;
            this.Parameters = method.GetParameters();
            this.ParameterTypes = method.GetParameters().Select(item => item.ParameterType).ToArray();
            this.ServiceAttribute = Attribute.GetCustomAttribute(method, typeof(ServiceAttribute)) as ServiceAttribute;

            var mFilters = Attribute.GetCustomAttributes(method, typeof(FilterAttribute), true) as FilterAttribute[];
            var cFilters = Attribute.GetCustomAttributes(method.DeclaringType, typeof(FilterAttribute), true) as FilterAttribute[];

            var hashSet = new HashSet<FilterAttribute>(mFilters);
            foreach (var filter in cFilters)
            {
                hashSet.Add(filter);
            }
            this.Filters = hashSet.OrderBy(item => item.Order).ToArray();
        }

        /// <summary>
        /// 是否声明特性
        /// </summary>
        /// <param name="type">特性类型</param>
        /// <param name="inherit">是否继承</param>
        /// <returns></returns>
        public bool IsDefined(Type type, bool inherit)
        {
            return this.Method.IsDefined(type, inherit) || this.Method.DeclaringType.IsDefined(type, inherit);
        }
    }
}
