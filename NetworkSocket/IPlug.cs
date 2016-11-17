using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 定义插件行为
    /// </summary>
    public interface IPlug
    {
        /// <summary>
        /// 会话连接后事件
        /// </summary>
        void OnConnected(object sender, IContenxt context);

        /// <summary>
        /// 会话断开后事件
        /// </summary>
        void OnDisconnected(object sender, IContenxt context);

        /// <summary>
        /// 服务异常事件
        /// </summary>
        void OnException(object sender, Exception exception);
    }
}
