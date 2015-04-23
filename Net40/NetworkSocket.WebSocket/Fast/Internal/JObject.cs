using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 表示动态Json对象
    /// </summary>
    internal class JObject : DynamicObject, IEnumerable<object>
    {
        /// <summary>
        /// 数组的数据
        /// </summary>
        private object[] _array;

        /// <summary>
        /// 原始数据
        /// </summary>
        private IDictionary<string, object> _sourceData;

        /// <summary>
        /// 获取是否为数组
        /// </summary>
        public bool IsArray
        {
            get
            {
                return this._array != null;
            }
        }

        /// <summary>
        /// 获取元素数量
        /// </summary>
        public int Length
        {
            get
            {
                if (this.IsArray)
                {
                    return this._array.Length;
                }
                return 1;
            }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                if (this.IsArray)
                {
                    return this._array[index];
                }
                return this;
            }
        }

        /// <summary>
        /// 创建动态Json数组对象
        /// </summary>
        /// <param name="array">Object对象或JObject对象</param>
        /// <exception cref="ArgumentNullException"></exception>
        private JObject(params object[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }
            this._array = array;
        }

        /// <summary>
        /// 表示动态Json对象
        /// </summary>
        /// <param name="data">内容字典</param>
        private JObject(IDictionary<string, object> data)
        {
            this._sourceData = data;
        }

        /// <summary>
        /// 转换为目标类型
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = JObject.Cast(this, binder.Type);
            return true;
        }


        /// <summary>
        /// 获取成员的值
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var key = binder.Name;
            this._sourceData.TryGetValue(key, out result);
            result = this.CastResult(result);
            return true;
        }

        /// <summary>
        /// 转换结果为JObject结构或JArray结构
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private object CastResult(object result)
        {
            if (result == null)
            {
                return null;
            }

            var dicResult = result as IDictionary<string, object>;
            if (dicResult != null)
            {
                return new JObject(dicResult);
            }

            var arrayResult = result as ArrayList;
            if (arrayResult != null)
            {
                var objectArray = arrayResult.Cast<object>().Select(item => CastResult(item)).ToArray();
                return new JObject(objectArray);
            }
            return result;
        }


        /// <summary>
        /// 解析Json
        /// </summary>
        /// <param name="json">json</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static dynamic Parse(string json)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
            return serializer.Deserialize(json, typeof(object));
        }

        /// <summary>
        /// 尝试将解析出来的动态值转换为目标类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static object TryCast<T>(object value)
        {
            try
            {
                return JObject.Cast<T>(value);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 将解析出来的动态值转换为目标类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">动态值</param>
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        public static T Cast<T>(object value)
        {
            return (T)JObject.Cast(value, typeof(T));
        }

        /// <summary>
        /// 将解析出来的动态值转换为目标类型
        /// </summary>
        /// <param name="value">动态值</param>
        /// <param name="targetType">目标类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        public static object Cast(object value, Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (targetType == typeof(object))
            {
                return value;
            }

            if (value == null)
            {
                if (targetType.IsValueType)
                {
                    return Activator.CreateInstance(targetType);
                }
                return null;
            }

            if (targetType == value.GetType())
            {
                return value;
            }

            try
            {
                return JObject.CastByJavaScriptSerializer(value, targetType);
            }
            catch (Exception ex)
            {
                throw new SerializerException(ex);
            }
        }

        /// <summary>
        /// 调用JavaScriptSerializer进行类型转换
        /// </summary>
        /// <param name="value">动态值</param>
        /// <param name="targetType">目标类型</param>     
        /// <returns></returns>
        private static object CastByJavaScriptSerializer(object value, Type targetType)
        {
            var serializer = new JavaScriptSerializer();
            var jObjectValue = value as JObject;

            if (jObjectValue == null)
            {
                return serializer.ConvertToType(value, targetType);
            }

            if (jObjectValue.IsArray == false)
            {
                value = jObjectValue._sourceData;
            }
            else
            {
                value = jObjectValue.Select(item => ((JObject)item)._sourceData).ToArray();
            }
            return serializer.ConvertToType(value, targetType);
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<object> GetEnumerator()
        {
            if (this.IsArray)
            {
                foreach (var item in this._array)
                {
                    yield return item;
                }
            }
            else
            {
                yield return this;
            }
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Json转换器
        /// </summary>
        private class DynamicJsonConverter : JavaScriptConverter
        {
            /// <summary>
            /// 获取支持的类型
            /// </summary>
            public override IEnumerable<Type> SupportedTypes
            {
                get
                {
                    yield return typeof(object);
                }
            }

            /// <summary>
            /// 不作序列化
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="dictionary"></param>
            /// <param name="type"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                return new JObject(dictionary);
            }
        }
    }
}
