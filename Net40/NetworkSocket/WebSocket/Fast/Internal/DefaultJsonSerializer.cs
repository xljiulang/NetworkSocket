using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

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
    }
}
