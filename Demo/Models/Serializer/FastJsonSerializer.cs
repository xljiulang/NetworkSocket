using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fastJSON;

namespace Models.Serializer
{
    public class FastJsonSerializer : NetworkSocket.Fast.ISerializer
    {
        public byte[] Serialize(object model)
        {
            var json = JSON.ToJSON(model);
            return Encoding.UTF8.GetBytes(json);
        }

        public object Deserialize(byte[] bytes, Type type)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JSON.ToObject(json, type);
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FastJsonSerializer";
        }
    }
}
