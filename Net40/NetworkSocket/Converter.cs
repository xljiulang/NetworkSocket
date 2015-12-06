using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 提供类型转换
    /// </summary>
    public sealed class Converter
    {
        /// <summary>
        /// 转换器静态实例
        /// </summary>
        private static readonly Converter Instance = new Converter();

        /// <summary>       
        /// 支持基础类型、decimal、guid和枚举相互转换以及这些类型的可空类型和数组类型相互转换
        /// 支持字典和DynamicObject转换为对象以及字典和DynamicObject的数组转换为对象数组
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">值</param>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public static T TryCast<T>(object value)
        {
            try
            {
                return Converter.Cast<T>(value);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>       
        /// 支持基础类型、decimal、guid和枚举相互转换以及这些类型的可空类型和数组类型相互转换
        /// 支持字典和DynamicObject转换为对象以及字典和DynamicObject的数组转换为对象数组
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">值</param>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public static T Cast<T>(object value)
        {
            return Converter.Instance.Convert<T>(value);
        }

        /// <summary>
        /// 支持基础类型、decimal、guid和枚举相互转换以及这些类型的可空类型和数组类型相互转换
        /// 支持字典和DynamicObject转换为对象以及字典和DynamicObject的数组转换为对象数组
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="targetType">目标类型</param>       
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public static object Cast(object value, Type targetType)
        {
            return Converter.Instance.Convert(value, targetType);
        }


        /// <summary>
        /// 转换执行者
        /// </summary>
        private List<IConvert> converts = new List<IConvert>();

        /// <summary>
        /// 类型转换
        /// </summary>
        public Converter()
            : this(null)
        {
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="customConverts">自定义的转换执行者</param>
        public Converter(params IConvert[] customConverts)
        {
            if (customConverts != null)
            {
                this.converts.AddRange(customConverts);
            }
            this.converts.Add(new NullConvert());
            this.converts.Add(new SippleConvert());
            this.converts.Add(new NullableConvert());
            this.converts.Add(new DictionaryConvert());           
            this.converts.Add(new ArrayConvert());
            this.converts.Add(new DynamicObjectConvert());
        }

        /// <summary>
        /// 转换为目标类型
        /// </summary>
        /// <typeparam name="T">要转换的目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的值</returns>
        public T Convert<T>(object value)
        {
            return (T)this.Convert(value, typeof(T));
        }

        /// <summary>
        /// 转换为目标类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <param name="targetType">要转换的目标类型</param>
        /// <returns>转换后的值</returns>
        public object Convert(object value, Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (targetType == typeof(object))
            {
                return value;
            }

            if (value != null && targetType == value.GetType())
            {
                return value;
            }

            object result;
            foreach (var item in this.converts)
            {
                if (item.Convert(this, value, targetType, out result) == true)
                {
                    return result;
                }
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// null值转换单元
        /// </summary>
        private class NullConvert : IConvert
        {
            public bool Convert(Converter converter, object value, Type targetType, out object result)
            {
                result = null;
                if (value != null)
                {
                    return false;
                }

                if (targetType.IsValueType == true)
                {
                    if (targetType.IsGenericType == false || targetType.GetGenericTypeDefinition() != typeof(Nullable<>))
                    {
                        throw new NotSupportedException();
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 简单类型转换单元
        /// </summary>
        private class SippleConvert : IConvert
        {
            public bool Convert(Converter converter, object value, Type targetType, out object result)
            {
                var convertible = value as IConvertible;
                if (convertible != null)
                {
                    result = convertible.ToType(targetType, null);
                    return true;
                }

                var valueString = value.ToString();
                if (typeof(Guid) == targetType)
                {
                    result = Guid.Parse(valueString);
                    return true;
                }
                else if (typeof(string) == targetType)
                {
                    result = valueString;
                    return true;
                }
                else if (targetType.IsEnum == true)
                {
                    result = Enum.Parse(targetType, valueString, true);
                    return true;
                }

                result = null;
                return false;
            }
        }

        /// <summary>
        /// 可空类型转换单元
        /// </summary>
        private class NullableConvert : IConvert
        {
            public bool Convert(Converter converter, object value, Type targetType, out object result)
            {
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var genericArgument = targetType.GetGenericArguments().First();
                    result = converter.Convert(value, genericArgument);
                    return true;
                }

                result = null;
                return false;
            }
        }

        /// <summary>
        /// 数组转换单元
        /// </summary>
        private class ArrayConvert : IConvert
        {
            public bool Convert(Converter converter, object value, Type targetType, out object result)
            {
                if (targetType.IsArray == false)
                {
                    result = null;
                    return false;
                }

                var items = value as IEnumerable;
                var elementType = targetType.GetElementType();

                if (items == null)
                {
                    result = Array.CreateInstance(elementType, 0);
                    return true;
                }

                var length = 0;
                var list = items as IList;
                if (list != null)
                {
                    length = list.Count;
                }
                else
                {
                    var enumerator = items.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        length = length + 1;
                    }
                }

                var index = 0;
                var array = Array.CreateInstance(elementType, length);
                foreach (var item in items)
                {
                    var itemCast = converter.Convert(item, elementType);
                    array.SetValue(itemCast, index);
                    index = index + 1;
                }

                result = array;
                return true;
            }
        }

        /// <summary>
        /// 字典转换单元
        /// </summary>
        private class DictionaryConvert : IConvert
        {
            public bool Convert(Converter converter, object value, Type targetType, out object result)
            {
                var dic = value as IDictionary<string, object>;
                if (dic == null)
                {
                    result = null;
                    return false;
                }

                var instance = Activator.CreateInstance(targetType);
                var setters = targetType.GetProperties().Where(item => item.CanWrite);

                foreach (var set in setters)
                {
                    var key = dic.Keys.FirstOrDefault(k => string.Equals(k, set.Name, StringComparison.OrdinalIgnoreCase));
                    if (key != null)
                    {
                        var targetValue = converter.Convert(dic[key], set.PropertyType);
                        set.SetValue(instance, targetValue, null);
                    }
                }

                result = instance;
                return true;
            }
        }

        /// <summary>
        /// 动态类型转换单元
        /// </summary>
        private class DynamicObjectConvert : IConvert
        {
            private class GetBinder : GetMemberBinder
            {
                public GetBinder(string name, bool ignoreCase)
                    : base(name, ignoreCase)
                {
                }
                public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                {
                    throw new NotImplementedException();
                }
            }

            private bool TryGetValue(DynamicObject dynamicObject, string key, out object value)
            {
                var keys = dynamicObject.GetDynamicMemberNames();
                key = keys.FirstOrDefault(item => string.Equals(item, key, StringComparison.OrdinalIgnoreCase));

                if (key != null)
                {
                    return dynamicObject.TryGetMember(new GetBinder(key, false), out value);
                }

                value = null;
                return false;
            }

            public bool Convert(Converter converter, object value, Type targetType, out object result)
            {
                var dynamicObject = value as DynamicObject;
                if (dynamicObject == null)
                {
                    result = null;
                    return false;
                }

                var instance = Activator.CreateInstance(targetType);
                var setters = targetType.GetProperties().Where(item => item.CanWrite);

                foreach (var set in setters)
                {
                    object targetValue;
                    if (this.TryGetValue(dynamicObject, set.Name, out targetValue) == true)
                    {
                        targetValue = converter.Convert(targetValue, set.PropertyType);
                        set.SetValue(instance, targetValue, null);
                    }
                }

                result = instance;
                return true;
            }
        }
    }
}
