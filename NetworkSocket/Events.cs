using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示连接后的委托
    /// </summary>
    /// <param name="sender">服务</param>
    /// <param name="context">上下文</param>
    public delegate void ConnectedHandler(object sender, IContenxt context);

    /// <summary>
    /// 表示断开连接后的委托
    /// </summary>
    /// <param name="sender">服务</param>
    /// <param name="context">上下文</param>
    public delegate void DisconnectedHandler(object sender, IContenxt context);

    /// <summary>
    /// 表示异常时的委托
    /// </summary>
    /// <param name="sender">服务</param>
    /// <param name="exception">异常</param>
    public delegate void ExceptionHandler(object sender, Exception exception);

    /// <summary>
    /// 表示事件管理器
    /// </summary>
    public class Events
    {
        /// <summary>
        /// 会话连接后事件
        /// </summary>
        public event ConnectedHandler OnConnected;

        /// <summary>
        /// 会话断开后事件
        /// </summary>
        public event DisconnectedHandler OnDisconnected;

        /// <summary>
        /// 服务异常事件
        /// </summary>
        public event ExceptionHandler OnException;

        /// <summary>
        /// 触发OnConnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="context"></param>
        internal void RaiseConnected(object sender, IContenxt context)
        {
            var @event = this.OnConnected;
            if (@event != null)
            {
                @event.Invoke(sender, context);
            }
        }

        /// <summary>
        /// 触发OnDisconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="context"></param>
        internal void RaiseDisconnected(object sender, IContenxt context)
        {
            var @event = this.OnDisconnected;
            if (@event != null)
            {
                @event.Invoke(sender, context);
            }
        }

        /// <summary>
        /// 触发OnException
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        internal void RaiseException(object sender, Exception exception)
        {
            var @event = this.OnException;
            if (@event != null)
            {
                @event.Invoke(sender, exception);
            }
        }
    }
}
