using NetworkSocket.Validation.Rules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 提供实体验证的态类  
    /// </summary>
    public static class Model
    {
        /// <summary>
        /// 为模型进行配置Fluent验证规则
        /// 将和Attribute规则协同生效
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns></returns>
        public static FluentApi<T> Fluent<T>()
        {
            return new FluentApi<T>();
        }

        /// <summary>
        /// 为匿名模型进行配置Fluent验证规则
        /// 将和Attribute规则协同生效
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="instance">匿名实例</param>
        /// <returns></returns>
        public static FluentApi<T> Fluent<T>(T instance)
        {
            return new FluentApi<T>();
        }

        /// <summary>
        /// 验证模型
        /// 包括Attribute规则和Fluent规则
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="model">模型实例</param>        
        /// <returns></returns>
        public static ValidResult ValidFor<T>(T model)
        {
            if (model == null)
            {
                return ValidResult.False("模型不能为null ..");
            }

            var context = new ValidContext(model, Property.GetProperties(typeof(T)));
            foreach (var property in context.Properties)
            {
                var failureRule = property.GetFailureRule(context);
                if (failureRule != null)
                {
                    var message = failureRule.FormatErrorMessage(null);
                    return ValidResult.False(message, property.Source);
                }
            }
            return ValidResult.True();
        }
    }
}