using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Models.Serializer
{
    public class GoogleProtobuf : NetworkSocket.Fast.ISerializer
    {
        public byte[] Serialize(object model)
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.NonGeneric.Serialize(stream, model);
                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] bytes, Type type)
        {
            using (var stream = new MemoryStream(bytes))
            {
                stream.Position = 0;
                return ProtoBuf.Serializer.NonGeneric.Deserialize(type, stream);
            }
        }
    }
}
