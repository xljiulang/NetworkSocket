using NetworkSocket.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 默认提供的Json序列化工具
    /// </summary>
    internal sealed class DefaultJsonSerializer : IJsonDynamicSerializer
    {
        /// <summary>
        /// 序列化为Json
        /// </summary>
        /// <param name="model">实体</param>
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        public string Serialize(object model)
        {
            if (model == null)
            {
                return null;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                return serializer.Serialize(model);
            }
            catch (Exception ex)
            {
                throw new SerializerException(ex);
            }
        }


        /// <summary>
        /// 反序列化json为动态类型
        /// 异常时抛出SerializerException
        /// </summary>
        /// <param name="json">json数据</param>      
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        public dynamic Deserialize(string json)
        {
            try
            {
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                throw new SerializerException(ex);
            }
        }

        /// <summary>
        /// 将值转换为目标类型
        /// 这些值有可能是反序列化得到的动态类型的值
        /// </summary>       
        /// <param name="value">要转换的值，可能</param>
        /// <param name="targetType">转换的目标类型</param>   
        /// <returns>转换结果</returns>
        public object Convert(object value, Type targetType)
        {
            // JObject解析JSON得到动态类型是DynamicObject
            // 默认的Converter实例能转换
            // 如果要加入其它转换单元，请使用new Converter(params IConvert[] customConverts)
            return Converter.Cast(value, targetType);
        }


        #region JObject
        /// <summary>
        /// 表示动态Json对象
        /// </summary>   
        [DebuggerTypeProxy(typeof(DebugView))]
        private class JObject : DynamicObject
        {
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
            /// 数据字典
            /// </summary>
            private IDictionary<string, object> data;

            /// <summary>
            /// 表示动态Json对象
            /// </summary>
            /// <param name="data">内容字典</param>
            private JObject(IDictionary<string, object> data)
            {
                this.data = data;
            }

            /// <summary>
            /// 获取成员名称
            /// </summary>
            /// <returns></returns>
            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return this.data.Keys;
            }

            /// <summary>
            /// 转换为目标类型
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryConvert(ConvertBinder binder, out object result)
            {
                try
                {
                    result = Converter.Cast(this, binder.Type);
                    return true;
                }
                catch (Exception)
                {
                    result = null;
                    return false;
                }
            }

            /// <summary>
            /// 获取成员的值
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = null;
                var key = this.data.Keys.FirstOrDefault(item => string.Equals(binder.Name, item, StringComparison.OrdinalIgnoreCase));
                if (key == null)
                {
                    return true;
                }

                object value;
                if (this.data.TryGetValue(key, out value) == false)
                {
                    return true;
                }

                result = this.CastToJObject(value);
                return true;
            }

            /// <summary>
            /// 转换结果为JObject结构或JArray结构
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            private object CastToJObject(object result)
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

                var listResult = result as IList;
                if (listResult != null)
                {
                    for (var i = 0; i < listResult.Count; i++)
                    {
                        var castValue = this.CastToJObject(listResult[i]);
                        listResult[i] = castValue;
                    }
                }
                return result;
            }

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
                public KeyValuePair<string, object>[] Values
                {
                    get
                    {
                        return view.data.ToArray();
                    }
                }
            }

            #endregion
        }
        #endregion
    }
}
