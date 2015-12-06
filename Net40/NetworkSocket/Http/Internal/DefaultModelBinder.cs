using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 默认的模型生成器
    /// </summary>
    internal class DefaultModelBinder : IModelBinder
    {
        /// <summary>
        /// 生成模型
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="parameter">参数</param>       
        /// <returns></returns>
        public object BindModel(HttpRequest request, ParameterInfo parameter)
        {
            var name = parameter.Name;
            var targetType = parameter.ParameterType;

            if (targetType.IsClass == true && targetType.IsArray == false && targetType != typeof(string))
            {
                return ConvertToClass(request, parameter);
            }

            // 转换为数组
            var values = request.GetValues(name);
            if (targetType.IsArray == true)
            {
                return Converter.Cast(values, targetType);
            }

            // 转换为简单类型 保留参数默认值
            var value = values.FirstOrDefault();
            if (value == null && parameter.DefaultValue != DBNull.Value)
            {
                return parameter.DefaultValue;
            }
            return Converter.Cast(value, targetType);
        }

        /// <summary>
        /// 转换为类
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <param name="request">请求数据</param>
        /// <returns></returns>
        public static object ConvertToClass(HttpRequest request, ParameterInfo parameter)
        {
            var targetType = parameter.ParameterType;
            if (targetType.IsByRef && targetType.IsInterface && targetType.IsAbstract)
            {
                throw new NotSupportedException("不支持的类型：" + targetType.Name);
            }

            var instance = Activator.CreateInstance(targetType);
            var setters = PropertySetter.GetPropertySetters(targetType);
            foreach (var setter in setters)
            {
                var value = request.GetValues(setter.Name).FirstOrDefault();
                if (value != null)
                {
                    var valueCast = Converter.Cast(value, setter.Type);
                    setter.SetValue(instance, valueCast);
                }
            }
            return instance;
        }
    }
}
