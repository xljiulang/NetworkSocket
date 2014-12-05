using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 默认提供的Json序列化工具
    /// </summary>
    internal sealed class DefaultSerializer : ISerializer
    {
        /// <summary>
        /// 序列化为二进制
        /// </summary>
        /// <param name="model">实体</param>
        /// <returns></returns>
        public byte[] Serialize(object model)
        {
            if (model == null)
            {
                return null;
            }

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(model);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// 反序列化为实体
        /// </summary>
        /// <param name="bytes">数据</param>
        /// <param name="type">实体类型</param>
        /// <returns></returns>
        public object Deserialize(byte[] bytes, Type type)
        {
            if (bytes == null || bytes.Length == 0 || type == null)
            {
                return null;
            }

            var json = Encoding.UTF8.GetString(bytes);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize(json, type);
        }
    }
}
