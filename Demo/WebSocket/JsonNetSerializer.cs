using NetworkSocket;
using NetworkSocket.Converts;
using NetworkSocket.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket
{
    /// <summary>
    /// 使用Json.net提供的序列化工具
    /// </summary>
    internal class JsonNetSerializer : IJsonSerializer
    {
        /// <summary>
        /// 转换器
        /// </summary>
        private readonly Converter converter = new Converter();

        /// <summary>
        /// Json.net提供的序列化工具
        /// </summary>
        public JsonNetSerializer()
        {
            this.converter.Items.AddFrist<JsonNetConvert>();
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Serialize(object model)
        {
            return JsonConvert.SerializeObject(model);
        }

        /// <summary>
        /// 反序列化为动态类型
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public dynamic Deserialize(string json)
        {
            return JObject.Parse(json);
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType)
        {
            return this.converter.Convert(value, targetType);
        }


        /// <summary>
        /// JsonNet的动态类型转换单元
        /// </summary>
        private class JsonNetConvert : IConvert
        {
            /// <summary>
            /// 只转换Json.Net的几个动态类型
            /// 这些类型都从JToken派生了
            /// </summary>
            /// <param name="converter"></param>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public bool Convert(Converter converter, object value, Type targetType, out object result)
            {
                var jToken = value as JToken;
                if (jToken == null)
                {
                    result = null;
                    return false;
                }
                result = jToken.ToObject(targetType);
                return true;
            }
        }
    }
}
