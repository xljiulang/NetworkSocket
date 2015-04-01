using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace NetworkSocket.Fast.Methods
{
    /// <summary>
    /// 通过表达式生成方法调用的委托
    /// 并将委托缓存
    /// </summary>
    internal class FastMethod
    {
        /// <summary>
        /// 委托
        /// </summary>
        private Func<object, object[], object> handler;

        /// <summary>
        /// 获取方法信息
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// 通过表达式生成方法调用的委托
        /// </summary>
        /// <param name="method">方法信息</param>
        public FastMethod(MethodInfo method)
        {
            this.Method = method;
            this.handler = this.CreateHandler(method);
        }

        /// <summary>
        /// 动态执行方法
        /// </summary>
        /// <param name="instance">方法所在的实例</param>
        /// <param name="parameters">方法的参数</param>
        /// <returns></returns>
        public object Invoke(object instance, object[] parameters)
        {
            return this.handler.Invoke(instance, parameters);
        }

        /// <summary>
        /// 生成方法的委托
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns></returns>
        private Func<object, object[], object> CreateHandler(MethodInfo method)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var parameters = Expression.Parameter(typeof(object[]), "parameters");

            var instanceCast = method.IsStatic ? null : Expression.Convert(instance, method.ReflectedType);
            var parametersCast = method.GetParameters()
                .Select((item, i) => Expression.Convert(Expression.ArrayIndex(parameters, Expression.Constant(i)), item.ParameterType));

            var body = Expression.Call(instanceCast, method, parametersCast);

            if (method.ReturnType == typeof(void))
            {
                var action = Expression.Lambda<Action<object, object[]>>(body, instance, parameters).Compile();
                return (obj, p) =>
                {
                    action.Invoke(obj, p);
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
        /// 获取哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Method.GetHashCode();
        }

        /// <summary>
        /// 是否与目标相等
        /// </summary>
        /// <param name="obj">目标</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }

        /// <summary>
        /// 方法名
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Method.Name;
        }
    }
}
