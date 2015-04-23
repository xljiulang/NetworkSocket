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
            try
            {
                var json = JSON.ToJSON(model);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                throw new NetworkSocket.Fast.SerializerException(ex);
            }
        }

        public object Deserialize(byte[] bytes, Type type)
        {
            try
            {
                var json = Encoding.UTF8.GetString(bytes);
                return JSON.ToObject(json, type);
            }
            catch (Exception ex)
            {
                throw new NetworkSocket.Fast.SerializerException(ex);
            }
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
