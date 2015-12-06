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
            var type = parameter.ParameterType;

            if (type.IsClass == true && type.IsArray == false && type != typeof(string))
            {
                return ConvertToClass(request, parameter);
            }

            var values = request.GetValues(name);
            if (type.IsArray == true)
            {
                return ConvertToArray(type, name, values);
            }

            return ConvertToSimple(type, name, values.FirstOrDefault(), parameter.DefaultValue);
        }

        /// <summary>
        /// 转换为类
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <param name="request">请求数据</param>
        /// <returns></returns>
        public static object ConvertToClass(HttpRequest request, ParameterInfo parameter)
        {
            var type = parameter.ParameterType;
            if (type.IsByRef && type.IsInterface && type.IsAbstract)
            {
                throw new NotSupportedException("不支持的类型：" + type.Name);
            }

            var instance = Activator.CreateInstance(type);
            var setters = PropertySetter.GetPropertySetters(type);
            foreach (var setter in setters)
            {
                var value = request.GetValues(setter.Name).FirstOrDefault();
                if (value != null)
                {
                    var valueCast = ConvertToSimple(setter.Type, setter.Name, value, DBNull.Value);
                    setter.SetValue(instance, valueCast);
                }
            }
            return instance;
        }

        /// <summary>
        /// 转换为数组
        /// </summary>
        /// <param name="type">数组类型</param>
        /// <param name="name">名称</param>
        /// <param name="values">值</param>
        /// <returns></returns>
        private static object ConvertToArray(Type type, string name, IList<string> values)
        {
            var elementType = type.GetElementType();
            if (values.Count == 0)
            {
                return Array.CreateInstance(elementType, 0);
            }

            var array = Array.CreateInstance(type.GetElementType(), values.Count);
            for (var i = 0; i < array.Length; i++)
            {
                var elementValue = ConvertToSimple(elementType, name, values[i], DBNull.Value);
                array.SetValue(elementValue, i);
            }
            return array;
        }

        /// <summary>
        /// 转换为简单类型
        /// </summary>       
        /// <param name="type">目标类型</param>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        private static object ConvertToSimple(Type type, string name, string value, object defaultValue)
        {
            if (value == null)
            {
                if (type.IsValueType && type.IsGenericType == false && defaultValue == DBNull.Value)
                {
                    throw new ArgumentNullException(name);
                }
                return DBNull.Value == defaultValue ? null : defaultValue;
            }

            if (typeof(IConvertible).IsAssignableFrom(type) == true)
            {
                return ((IConvertible)value).ToType(type, null);
            }

            if (typeof(Guid) == type)
            {
                return Guid.Parse(value);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var argTypes = type.GetGenericArguments();
                if (argTypes.Length == 1)
                {
                    return ConvertToSimple(argTypes.First(), name, value, DBNull.Value);
                }
            }

            throw new NotSupportedException("不支持的类型：" + type.Name);
        }
    }
}
