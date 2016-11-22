using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 定义服务器插件行为
    /// </summary>
    public interface IPlug
    {
        /// <summary>
        /// 会话连接成功后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        void OnConnected(object sender, IContenxt context);

        /// <summary>
        /// SSL验证完成后触发
        /// 如果起用了SSL，验证通过后才可以往客户端发送数据
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        void OnAuthenticated(object sender, IContenxt context);

        /// <summary>
        /// 收到请求后触发
        /// 此方法在先于协议中间件的Invoke方法调用
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        void OnRequested(object sender, IContenxt context);

        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        void OnDisconnected(object sender, IContenxt context);

        /// <summary>
        /// 服务异常后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="exception">异常</param>
        void OnException(object sender, Exception exception);
    }
}
