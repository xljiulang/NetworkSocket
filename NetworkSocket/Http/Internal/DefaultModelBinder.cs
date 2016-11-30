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
            var isRawJson = context.Request.IsRawJsonRequest(out chartset);

            foreach (var parameter in context.Action.Parameters)
            {
                if (isRawJson && parameter.IsDefined<BodyAttribute>() == true)
                {
                    parameter.Value = this.GenerateModelFromBody(context.Request, parameter, chartset);
                }
                else
                {
                    parameter.Value = this.GenerateModelFromQueryForm(context.Request, parameter);
                }
            }
        }

        /// <summary>
        /// 将请求的body转换为参数模型
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="parameter">参数</param>
        /// <param name="chartset">编码</param>
        /// <returns></returns>
        private object GenerateModelFromBody(HttpRequest request, ApiParameter parameter, Encoding chartset)
        {
            var json = chartset.GetString(request.Body);
            var body = new DefaultDynamicJsonSerializer().Deserialize(json, typeof(object));

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
        /// 将请求Form或Query转换为参数模型
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="parameter">参数</param>       
        /// <returns></returns>
        private object GenerateModelFromQueryForm(HttpRequest request, ApiParameter parameter)
        {
            var name = parameter.Name;
            var targetType = parameter.Type;

            if (targetType.IsComplexClass() == true)
            {
                return this.QueryFormToComplex(request, parameter);
            }

            // 转换为数组
            var values = request.GetValues(name);
            if (targetType.IsArrayOrList() == true)
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
        /// 表单转换为复杂对象
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="parameter">参数</param>       
        /// <returns></returns>
        private object QueryFormToComplex(HttpRequest request, ApiParameter parameter)
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
                    .Where(p => p.CanWrite && p.PropertyType.IsSimple())
                    .Select(p => new ModelProperty(p))
                    .ToArray();
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
