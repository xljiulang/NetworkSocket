using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    [DebuggerDisplay("IsArray = {IsArray}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    internal class JObject : DynamicObject, IDictionary<string, object>, IEnumerable<object>
    {
        /// <summary>
        /// 数据数组
        /// </summary>
        private object[] dataArray;

        /// <summary>
        /// 数据字典
        /// </summary>
        private IDictionary<string, object> dataDic;

        /// <summary>
        /// 获取是否为数组
        /// </summary>
        public bool IsArray
        {
            get
            {
                return this.dataArray != null;
            }
        }

        /// <summary>
        /// 获取数组长度
        /// </summary>
        public int Length
        {
            get
            {
                if (this.IsArray)
                {
                    return this.dataArray.Length;
                }
                return 1;
            }
        }

        /// <summary>
        /// 获取指定索引内容
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                return this.ToArray()[index];
            }
        }

        /// <summary>
        /// 创建动态Json数组对象
        /// </summary>
        /// <param name="dataArray">Object对象或JObject对象</param>
        /// <exception cref="ArgumentNullException"></exception>
        private JObject(params object[] dataArray)
        {
            if (dataArray == null)
            {
                throw new ArgumentNullException();
            }
            this.dataArray = dataArray;
        }

        /// <summary>
        /// 表示动态Json对象
        /// </summary>
        /// <param name="dataDic">内容字典</param>
        private JObject(IDictionary<string, object> dataDic)
        {
            this.dataDic = dataDic;
        }

        /// <summary>
        /// 转换为数组
        /// </summary>
        /// <returns></returns>
        public object[] ToArray()
        {
            if (this.IsArray)
            {
                return this.dataArray;
            }
            else
            {
                return new object[] { this };
            }
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<object> GetEnumerator()
        {
            var array = this.ToArray();
            foreach (var item in array)
            {
                yield return item;
            }
        }

        /// <summary>
        /// 迭代自身的元素
        /// 而不是字典的元素
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
            this.dataDic.TryGetValue(key, out result);
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
        public static T TryCast<T>(object value)
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
            var jObjectValue = value as JObject;
            if (jObjectValue != null && jObjectValue.IsArray)
            {
                value = jObjectValue.ToArray();
            }

            var serializer = new JavaScriptSerializer();
            return serializer.ConvertToType(value, targetType);
        }

        #region IDictionary

        void IDictionary<string, object>.Add(string key, object value)
        {
            this.dataDic.Add(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return this.dataDic.ContainsKey(key);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                return this.dataDic.Keys;
            }
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return this.dataDic.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return this.dataDic.TryGetValue(key, out value);
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                return this.dataDic.Values;
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                return this.dataDic[key];
            }
            set
            {
                this.dataDic[key] = value;
            }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            this.dataDic.Add(item);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            this.dataDic.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return this.dataDic.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this.dataDic.CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                return this.dataDic.Count;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                return this.dataDic.IsReadOnly;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return this.dataDic.Remove(item);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return this.dataDic.GetEnumerator();
        }
        #endregion

        #region DynamicJsonConverter
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
        #endregion

        #region DebugView
        /// <summary>
        /// 调试视图
        /// </summary>
        private class DebugView
        {
            /// <summary>
            /// 查看的对象
            /// </summary>
            private JObject view;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="view">查看的对象</param>
            public DebugView(JObject view)
            {
                this.view = view;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Values
            {
                get
                {
                    return view.ToArray();
                }
            }
        }

        #endregion
    }
}
