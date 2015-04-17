using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 服务行为    
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
    public class FastAction
    {
        /// <summary>
        /// 服务行为的方法成员信息
        /// </summary>
        private MethodInfo method;

        /// <summary>
        /// 服务行为的方法成员调用委托
        /// </summary>
        private Func<object, object[], object> methodInvoker;

        /// <summary>
        /// 获取服务行为的Api名称
        /// </summary>
        public string ApiName { get; private set; }

        /// <summary>
        /// 获取服务行为的方法成员返回类型是否为void
        /// </summary>
        public bool IsVoidReturn { get; private set; }

        /// <summary>
        /// 服务行为的方法成员返回类型
        /// </summary>
        public Type ReturnType { get; private set; }

        /// <summary>
        /// 获取服务行为的方法成员参数类型
        /// </summary>
        public Type[] ParameterTypes { get; private set; }

        /// <summary>
        /// 获取声明该成员的服务类型
        /// </summary>
        public Type DeclaringService { get; private set; }


        /// <summary>
        /// 服务行为
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <exception cref="ArgumentException"></exception>
        public FastAction(MethodInfo method)
        {
            this.method = method;
            this.methodInvoker = FastAction.CreateMethodInvoker(method);

            this.DeclaringService = method.DeclaringType;

            this.ReturnType = method.ReturnType;
            this.IsVoidReturn = method.ReturnType.Equals(typeof(void));
            this.ParameterTypes = method.GetParameters().Select(item => item.ParameterType).ToArray();

            var api = Attribute.GetCustomAttribute(method, typeof(ApiAttribute)) as ApiAttribute;
            this.ApiName = api.Name ?? method.Name;
        }

        /// <summary>
        /// 生成方法的委托
        /// </summary>
        /// <param name="method">方法成员信息</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        private static Func<object, object[], object> CreateMethodInvoker(MethodInfo method)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var parameters = Expression.Parameter(typeof(object[]), "parameters");

            var instanceCast = method.IsStatic ? null : Expression.Convert(instance, method.ReflectedType);
            var parametersCast = method.GetParameters().Select((p, i) =>
            {
                var parameter = Expression.ArrayIndex(parameters, Expression.Constant(i));
                return Expression.Convert(parameter, p.ParameterType);
            });

            var body = Expression.Call(instanceCast, method, parametersCast);

            if (method.ReturnType == typeof(void))
            {
                var action = Expression.Lambda<Action<object, object[]>>(body, instance, parameters).Compile();
                return (_instance, _parameters) =>
                {
                    action.Invoke(_instance, _parameters);
                    return null;
                };
            }
            else
            {
                var bodyCast = Expression.Convert(body, typeof(object));
                return Expression.Lambda<Func<object, object[], object>>(bodyCast, instance, parameters).Compile();
            }
        }


        /// <summary>
        /// 获取服务行为或服务行为的声明类型是否声明了特性
        /// </summary>
        /// <param name="type">特性类型</param>
        /// <param name="inherit">是否继承</param>
        /// <returns></returns>
        public bool IsDefined(Type type, bool inherit)
        {
            return this.method.IsDefined(type, inherit) || this.DeclaringService.IsDefined(type, inherit);
        }

        /// <summary>
        /// 获取方法级过滤器特性
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<FilterAttribute> GetMethodFilterAttributes()
        {
            return Attribute.GetCustomAttributes(this.method, typeof(FilterAttribute), true).Cast<FilterAttribute>();
        }

        /// <summary>
        /// 获取类级过滤器特性
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<FilterAttribute> GetClassFilterAttributes()
        {
            return Attribute.GetCustomAttributes(this.DeclaringService, typeof(FilterAttribute), true).Cast<FilterAttribute>();
        }

        /// <summary>
        /// 执行服务行为
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="parameters">参数实例</param>
        /// <returns></returns>
        public object Execute(object service, params object[] parameters)
        {
            return this.methodInvoker.Invoke(service, parameters);
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ApiName;
        }
    }
}
