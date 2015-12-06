using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using NetworkSocket.Validation.Rules;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 实体验证  
    /// </summary>
    public static class Model
    {
        /// <summary>
        /// 验证模型
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="model">模型实例</param>        
        /// <returns></returns>
        public static ValidResult ValidFor<T>(T model)
        {
            if (model == null)
            {
                return new ValidResult
                {
                    State = false,
                    ErrorMessage = "模型不能为null .."
                };
            }

            var validContext = new ValidContext
            {
                Instance = model,
                Properties = Property.GetProperties(typeof(T))
            };

            foreach (var property in validContext.Properties)
            {
                var value = property.GetValue(model);
                var failureRule = property.ValidRules.FirstOrDefault(r => r.IsValid(value, validContext) == false);

                if (failureRule == null)
                {
                    continue;
                }
                return new ValidResult
                {
                    ProperyName = property.Name,
                    ErrorMessage = failureRule.FormatErrorMessage(null)
                };
            }
            return new ValidResult { State = true };
        }
    }
}