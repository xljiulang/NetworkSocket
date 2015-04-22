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
    internal class JObject : DynamicObject
    {
        /// <summary>
        /// 键值内容字典
        /// </summary>
        private IDictionary<string, object> data;

        /// <summary>
        /// 缓存
        /// </summary>
        private Lazy<IDictionary<string, object>> cached = new Lazy<IDictionary<string, object>>(() => new Dictionary<string, object>());

        /// <summary>
        /// 表示动态Json对象
        /// </summary>
        /// <param name="data">内容字典</param>
        private JObject(IDictionary<string, object> data)
        {
            this.data = data;
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
            if (this.cached.Value.TryGetValue(key, out result))
            {
                return true;
            }

            this.data.TryGetValue(key, out result);
            result = this.CastResult(result);
            this.cached.Value[key] = result;
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
                return new JArray(objectArray);
            }
            return result;
        }

        /// <summary>
        /// 表示json对象数组
        /// </summary>
        internal class JArray : IEnumerable<object>
        {
            /// <summary>
            /// json对象数组
            /// </summary>
            private object[] array;

            /// <summary>
            /// 索引器
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public object this[int index]
            {
                get
                {
                    return array[index];
                }
            }

            /// <summary>
            /// 表示json对象数组
            /// </summary>
            /// <param name="objectArray">json对象数组</param>
            public JArray(object[] objectArray)
            {
                this.array = objectArray;
            }

            /// <summary>
            /// 获取迭代器
            /// </summary>
            /// <returns></returns>
            public IEnumerator<object> GetEnumerator()
            {
                foreach (var item in this.array)
                {
                    yield return item;
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
        /// 将解析出来的动态值转换为目标类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">动态值</param>
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

            var valueType = value.GetType();
            if (valueType == typeof(JObject))
            {
                value = ((JObject)value).data;
            }
            else if (valueType == typeof(JArray))
            {
                value = (value as JArray).Select(item => ((JObject)item).data).ToArray();
            }
            else
            {
                return Convert.ChangeType(value, targetType);
            }

            var serializer = new JavaScriptSerializer();
            return serializer.ConvertToType(value, targetType);
        }
    }
}
