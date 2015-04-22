using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
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
    }
}
