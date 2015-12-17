using NetworkSocket.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
        private static readonly ConcurrentDictionary<Type, Property[]> propertyCached = new ConcurrentDictionary<Type, Property[]>();

        /// <summary>
        /// 获取模型类型的属性
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        private static Property[] GetTypeProperties(Type modelType)
        {
            return modelType.GetProperties()
                .Where(item => item.CanRead)
                .Where(item => item.PropertyType == typeof(Guid) || typeof(IConvertible).IsAssignableFrom(item.PropertyType))
                .Select(item => new Property(item))
                .ToArray();
        }

        /// <summary>
        /// 获取类型的所有属性的get方法
        /// </summary>
        /// <param name="modelType">模型类型</param>       
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static Property[] GetProperties(Type modelType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException();
            }
            return propertyCached.GetOrAdd(modelType, (t) => Property.GetTypeProperties(t));
        }




        /// <summary>
        /// 属性的Get方法
        /// </summary>
        private Func<object, object[], object> getter;

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
        private Property(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException();
            }

            this.Source = property;
            this.Name = property.Name;
            this.ValidRules = this.GetValidRules(property);
            this.getter = MethodReflection.CreateInvoker(property.GetGetMethod());
        }

        /// <summary>
        /// 获取属性的验证规则
        /// </summary>
        /// <param name="property">属性</param>        
        /// <returns></returns>
        private IValidRule[] GetValidRules(PropertyInfo property)
        {
            return Attribute
                .GetCustomAttributes(property, false)
                .Where(item => item is IValidRule)
                .Cast<IValidRule>()
                .OrderBy(item => item.OrderIndex)
                .ToArray();
        }

        /// <summary>
        /// 增加或更新规则
        /// </summary>
        /// <param name="rule">验证规则</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetRule(IValidRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException();
            }

            if (this.Replace(rule) == false)
            {
                this.AddRule(rule);
            }
        }

        /// <summary>
        /// 增加验证规则
        /// </summary>
        /// <param name="rule">验证规则</param>
        private void AddRule(IValidRule rule)
        {
            this.ValidRules = this
                .ValidRules
                .Concat(new[] { rule })
                .OrderBy(item => item.OrderIndex)
                .ToArray();
        }

        /// <summary>
        /// 替换验证规则
        /// </summary>
        /// <param name="rule">验证规则</param>
        /// <returns></returns>
        private bool Replace(IValidRule rule)
        {
            for (var i = 0; i < this.ValidRules.Length; i++)
            {
                if (this.ValidRules[i].GetType() == rule.GetType())
                {
                    this.ValidRules[i] = rule;
                    return true;
                }
            }
            return false;
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
        /// 获取验证不通过的第一个规则
        /// 都通过则返回null
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public IValidRule GetFailureRule(ValidContext context)
        {
            if (this.ValidRules.Length == 0)
            {
                return null;
            }
            var value = this.GetValue(context.Instance);
            return this.ValidRules.FirstOrDefault(r => r.IsValid(value, context) == false);
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
