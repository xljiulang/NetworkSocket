using NetworkSocket.Reflection;
using NetworkSocket.Util;
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
                return this.ConvertToClass(request, parameter);
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
        public object ConvertToClass(HttpRequest request, ParameterInfo parameter)
        {
            var targetType = parameter.ParameterType;
            if (targetType.IsByRef && targetType.IsInterface && targetType.IsAbstract)
            {
                throw new NotSupportedException("不支持的类型：" + targetType.Name);
            }

            var instance = Activator.CreateInstance(targetType);
            var setters = ModelProperty.GetSetProperties(targetType);
            foreach (var setter in setters)
            {
                var value = request.GetValues(setter.Name).FirstOrDefault();
                if (value != null)
                {
                    var valueCast = Converter.Cast(value, setter.Info.PropertyType);
                    setter.SetValue(instance, valueCast);
                }
            }
            return instance;
        }

        /// <summary>
        /// 表示Model的属性
        /// </summary>
        private class ModelProperty : Property
        {
            /// <summary>
            /// 类型属性的Setter缓存
            /// </summary>
            private static readonly ConcurrentDictionary<Type, ModelProperty[]> cached = new ConcurrentDictionary<Type, ModelProperty[]>();

            /// <summary>
            /// 从类型的属性获取Set属性
            /// </summary>
            /// <param name="type">类型</param>
            /// <returns></returns>
            public static ModelProperty[] GetSetProperties(Type type)
            {
                return ModelProperty.cached.GetOrAdd(type, ModelProperty.GetSetPropertiesNoCached(type));
            }

            /// <summary>
            /// 从类型的属性获取Set属性
            /// </summary>
            /// <param name="type">类型</param>
            /// <returns></returns>
            private static ModelProperty[] GetSetPropertiesNoCached(Type type)
            {
                return type.GetProperties()
                    .Where(p => p.CanWrite && ModelProperty.IsSimpleType(p.PropertyType))
                    .Select(p => new ModelProperty(p))
                    .ToArray();
            }


            /// <summary>
            /// 类型是否为所支持的简单类型
            /// </summary>
            /// <param name="type">类型</param>
            /// <returns></returns>
            private static bool IsSimpleType(Type type)
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
                        return ModelProperty.IsSimpleType(argTypes.First());
                    }
                }
                return false;
            }

            /// <summary>
            /// 属性的Setter
            /// </summary>       
            /// <param name="property">属性</param>        
            public ModelProperty(PropertyInfo property)
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
}
