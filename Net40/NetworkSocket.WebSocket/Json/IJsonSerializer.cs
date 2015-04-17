using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Json
{
    /// <summary>
    /// 定义对象的序列化与反序列化的接口
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// 序列化为Json
        /// </summary>
        /// <param name="model">实体</param>
        /// <returns></returns>
        string Serialize(object model);

        /// <summary>
        /// 反序列化为实体
        /// </summary>
        /// <param name="json">json数据</param>
        /// <param name="type">实体类型</param>
        /// <returns></returns>
        object Deserialize(string json, Type type);
    }
}
