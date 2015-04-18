using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 默认提供的Json序列化工具
    /// </summary>
    internal sealed class DefaultJsonSerializer : IJsonSerializer
    {
        /// <summary>
        /// 序列化为Json
        /// </summary>
        /// <param name="model">实体</param>
        /// <returns></returns>
        public string Serialize(object model)
        {
            if (model == null)
            {
                return null;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(model);
        }

        /// <summary>
        /// 反序列化为实体
        /// </summary>
        /// <param name="json">Json数据</param>
        /// <param name="type">实体类型</param>
        /// <returns></returns>
        public object Deserialize(string json, Type type)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
        }
    }
}
