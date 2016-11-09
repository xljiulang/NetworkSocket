using NetworkSocket.Core;
using NetworkSocket.Reflection;
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
    public class RuleProperty : Property
    {
        /// <summary>
        /// 获取属性的验证规则
        /// </summary>
        public IValidRule[] ValidRules { get; private set; }

        /// <summary>
        /// 属性
        /// </summary>
        /// <param name="property">属性</param>
        /// <exception cref="ArgumentException"></exception>
        private RuleProperty(PropertyInfo property)
            : base(property)
        {
            if (property == null)
            {
                throw new ArgumentNullException();
            }
            this.ValidRules = this.GetValidRules(property);
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


        /// <summary>
        /// 类型属性的Setter缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, RuleProperty[]> cached = new ConcurrentDictionary<Type, RuleProperty[]>();

        /// <summary>
        /// 从类型的属性获取Get属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static RuleProperty[] GetGetProperties(Type type)
        {
            return cached.GetOrAdd(type, t => t.GetProperties().Where(item => item.CanRead).Select(p => new RuleProperty(p)).ToArray());
        }
    }
}
