using NetworkSocket.Core;
using NetworkSocket.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示属性的Setter
    /// </summary>
    internal class BinderProperty : Property
    {
        /// <summary>
        /// 类型属性的Setter缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, BinderProperty[]> cached = new ConcurrentDictionary<Type, BinderProperty[]>();

        /// <summary>
        /// 从类型的属性获取Set属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static BinderProperty[] GetSetProperties(Type type)
        {
            Func<Type, BinderProperty[]> func = (t) =>
                t.GetProperties()
                .Where(p => p.CanWrite && IsPrimitive(p.PropertyType))
                .Select(p => new BinderProperty(p))
                .ToArray();

            return BinderProperty.cached.GetOrAdd(type, func);
        }

        /// <summary>
        /// 类型是否为所支持的简单类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        private static bool IsPrimitive(Type type)
        {
            if (typeof(IConvertible).IsAssignableFrom(type) == true)
            {
                return true;
            }

            if (typeof(Guid) == type)
            {
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var argTypes = type.GetGenericArguments();
                if (argTypes.Length == 1)
                {
                    return IsPrimitive(argTypes.First());
                }
            }
            return false;
        }

        /// <summary>
        /// 属性的Setter
        /// </summary>       
        /// <param name="property">属性</param>        
        public BinderProperty(PropertyInfo property)
            : base(property)
        {
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
