using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NetworkSocket.Fast;

namespace Models.Serializer
{
    public class GoogleProtobuf : ISerializer
    {
        public byte[] Serialize(object model)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    ProtoBuf.Serializer.NonGeneric.Serialize(stream, model);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new SerializerException(ex);
            }
        }

        public object Deserialize(byte[] bytes, Type type)
        {
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    stream.Position = 0;
                    return ProtoBuf.Serializer.NonGeneric.Deserialize(type, stream);
                }
            }
            catch (Exception ex)
            {
                throw new SerializerException(ex);
            }
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ProtoBufSerializer";
        }
    }
}
