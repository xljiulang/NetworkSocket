using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;
using NetworkSocket.Core;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 表示反射优化过的属性    
    /// </summary>   
    public class Property
    {
        /// <summary>
        /// 类型的属性缓存
        /// </summary>
        private static ConcurrentDictionary<Type, Property[]> propertyCached = new ConcurrentDictionary<Type, Property[]>();

        /// <summary>
        /// 获取有类型带有验证规则特性的属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Property[] GetTypeValidPropertys(Type type)
        {
            return type.GetProperties()
                .Where(item => item.CanRead)
                .Where(item => item.PropertyType == typeof(Guid) || item.PropertyType.IsEnum || typeof(IConvertible).IsAssignableFrom(item.PropertyType))
                .Select(item => new Property(item))
                .Where(item => item.ValidRules.Length > 0)
                .ToArray();
        }

        /// <summary>
        /// 获取类型的所有属性的get方法
        /// </summary>
        /// <param name="type">类型</param>       
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static Property[] From(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }
            return propertyCached.GetOrAdd(type, (t) => GetTypeValidPropertys(t));
        }

        /// <summary>
        /// 属性的Get方法
        /// </summary>
        private Func<object, object[], object> getter;

        /// <summary>
        /// 属性的Set方法
        /// </summary>
        private Func<object, object[], object> setter;

        /// <summary>
        /// 获取属性名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取原始的属性
        /// </summary>
        public PropertyInfo Source { get; private set; }

        /// <summary>
        /// 获取属性的验证规则
        /// </summary>
        public IValidRule[] ValidRules { get; private set; }

        /// <summary>
        /// 属性
        /// </summary>
        /// <param name="property">属性</param>
        /// <exception cref="ArgumentException"></exception>
        public Property(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException();
            }

            this.Name = property.Name;
            this.Source = property;
            this.ValidRules = GetValidRules(property);

            if (property.CanRead == true)
            {
                this.getter = MethodReflection.CreateInvoker(property.GetGetMethod());
            }
            if (property.CanWrite == true)
            {
                this.setter = MethodReflection.CreateInvoker(property.GetSetMethod());
            }
        }

        /// <summary>
        /// 获取属性的验证规则
        /// </summary>
        /// <param name="property">属性</param>        
        /// <returns></returns>
        private static IValidRule[] GetValidRules(PropertyInfo property)
        {
            return Attribute
                .GetCustomAttributes(property, false)
                .Where(item => item is IValidRule)
                .Cast<IValidRule>()
                .OrderBy(item => item.OrderIndex)
                .ToArray();
        }

        /// <summary>
        /// 获取属性的值
        /// </summary>
        /// <param name="instance">实例</param>  
        /// <returns></returns>
        public object GetValue(object instance)
        {
            if (this.getter == null)
            {
                return null;
            }
            return this.getter.Invoke(instance, null);
        }

        /// <summary>
        /// 设置属性的值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="value">值</param>
        public void SetValue(object instance, object value)
        {
            if (this.setter != null)
            {
                this.setter.Invoke(instance, new[] { value });
            }
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
