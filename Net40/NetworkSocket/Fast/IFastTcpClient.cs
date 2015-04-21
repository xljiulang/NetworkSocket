using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp客户端接口
    /// </summary>
    public interface IFastTcpClient : ITcpClient, IFastSession
    {
        /// <summary>
        /// 获取或设置序列化工具      
        /// </summary>
        ISerializer Serializer { get; set; }
    }
}
