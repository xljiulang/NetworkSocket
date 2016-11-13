using NetworkSocket.Core;
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
        /// 生成和绑定所有参数的值
        /// </summary>
        /// <param name="context">上下文</param>
        public void BindAllParameterValue(ActionContext context)
        {
            Encoding chartset;
            if (context.Request.IsRawJsonRequest(out chartset))
            {
                this.BindParametersFromRawJson(context, chartset);
            }
            else
            {
                this.BindParametersFromForm(context);
            }
        }

        /// <summary>
        /// 生成和绑定所有参数的值
        /// Raw Json
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="chartset">字符编码</param>
        private void BindParametersFromRawJson(ActionContext context, Encoding chartset)
        {
            if (context.Action.Parameters.Length == 0)
            {
                return;
            }

            var json = chartset.GetString(context.Request.Body);
            var body = new DefaultDynamicJsonSerializer().Deserialize(json, typeof(object));
            var bodyLazy = new Lazy<IDictionary<string, object>>(() =>
            {
                var dic = body as IDictionary<string, object>;
                if (dic != null) dic = dic.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                return dic;
            });
            this.BindParametersFromRawJson(context, body, bodyLazy);
        }


        /// <summary>
        /// 生成和绑定所有参数的值     
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="body">body</param>
        /// <param name="bodyLazy">body字典</param>
        private void BindParametersFromRawJson(ActionContext context, object body, Lazy<IDictionary<string, object>> bodyLazy)
        {
            foreach (var parameter in context.Action.Parameters)
            {
                if (parameter.IsDefined<BodyAttribute>() == true)
                {
                    parameter.Value = this.GetModelFromBody(body, parameter);
                }
                else if (parameter.IsDefined<QueryAttribute>() == true)
                {
                    parameter.Value = this.GetValueFromQueryForm(context.Request, parameter);
                }
                else
                {
                    parameter.Value = this.GetValueFromBody(bodyLazy.Value, parameter);
                }
            }
        }


        /// <summary>
        /// 将body转换为模型
        /// </summary>
        /// <param name="body">原始数据</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        private object GetModelFromBody(object body, ApiParameter parameter)
        {
            if (body != null)
            {
                return Converter.Cast(body, parameter.Type);
            }

            var defaultValue = parameter.Info.DefaultValue;
            if (defaultValue == DBNull.Value)
            {
                defaultValue = null;
            }
            return defaultValue;
        }


        /// <summary>
        /// 从body数据获取参数值
        /// </summary>
        /// <param name="body">原始数据</param>
        /// <param name="parameter">参数</param>       
        /// <returns></returns>
        private object GetValueFromBody(IDictionary<string, object> body, ApiParameter parameter)
        {
            if (body != null)
            {
                object value = null;
                if (body.TryGetValue(parameter.Name, out value))
                {
                    return Converter.Cast(value, parameter.Type);
                }
            }

            var defaultValue = parameter.Info.DefaultValue;
            if (defaultValue == DBNull.Value)
            {
                defaultValue = null;
            }
            return defaultValue;
        }

        /// <summary>
        /// 生成和绑定所有参数的值
        /// form/query
        /// </summary>
        /// <param name="context">上下文</param>
        private void BindParametersFromForm(ActionContext context)
        {
            foreach (var parameter in context.Action.Parameters)
            {
                parameter.Value = this.GetValueFromQueryForm(context.Request, parameter);
            }
        }

        /// <summary>
        /// 从Form或Query获取参数值
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="parameter">参数</param>       
        /// <returns></returns>
        private object GetValueFromQueryForm(HttpRequest request, ApiParameter parameter)
        {
            var name = parameter.Name;
            var targetType = parameter.Type;

            if (targetType.IsClass == true && targetType.IsArray == false && targetType != typeof(string))
            {
                return this.QueryFormToClass(request, parameter);
            }

            // 转换为数组
            var values = request.GetValues(name);
            if (targetType.IsArray == true)
            {
                return Converter.Cast(values, targetType);
            }

            // 转换为简单类型 保留参数默认值
            var value = values.FirstOrDefault();
            if (value == null && parameter.Info.DefaultValue != DBNull.Value)
            {
                return parameter.Info.DefaultValue;
            }
            return Converter.Cast(value, targetType);
        }

        /// <summary>
        /// 表单转换为类
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="parameter">参数</param>       
        /// <returns></returns>
        private object QueryFormToClass(HttpRequest request, ApiParameter parameter)
        {
            var targetType = parameter.Type; 
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
