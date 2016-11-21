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
        /// 会话连接后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        void OnConnected(object sender, IContenxt context);

        /// <summary>
        /// SSL验证后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        /// <param name="exception">验证的异常，如果没有异常则为null</param>
        void OnSSLAuthenticated(object sender, IContenxt context, Exception exception);

        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        void OnDisconnected(object sender, IContenxt context);

        /// <summary>
        /// 服务异常事件
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="exception">异常</param>
        void OnException(object sender, Exception exception);
    }
}
