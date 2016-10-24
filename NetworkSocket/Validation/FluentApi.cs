using NetworkSocket.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 表示Fluent验证规则
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FluentApi<T>
    {
        /// <summary>
        /// Fluent验证规则
        /// </summary>
        internal FluentApi()
        {
        }

        /// <summary>
        /// 增加或替换验证规则到类型的属性
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="keySelector">属性选择</param>
        /// <param name="rule">验证规则</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public FluentApi<T> SetRule<TKey>(Expression<Func<T, TKey>> keySelector, IValidRule rule)
        {
            FluentApiExtend.GetProperty(keySelector).SetRule(rule);
            return this;
        }
    }
}
