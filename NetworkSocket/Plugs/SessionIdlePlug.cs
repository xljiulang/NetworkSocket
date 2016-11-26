using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Plugs
{
    /// <summary>
    /// 表示空闲会话检测与关闭插件
    /// </summary>
    public class SessionIdlePlug : PlugBase
    {
        /// <summary>
        /// 空闲时间超时检测timer
        /// </summary>
        private static readonly string IdleTimer = "NetworkSocket.IdleTimer";

        /// <summary>
        /// 获取最大空闲时间
        /// </summary>
        protected TimeSpan MaxIdleTime { get; private set; }

        /// <summary>
        /// 空闲会话检测与处理插件       
        /// </summary>
        /// <param name="maxIdleTime">最大空闲时间</param>
        public SessionIdlePlug(TimeSpan maxIdleTime)
        {
            this.MaxIdleTime = maxIdleTime;
        }

        /// <summary>
        /// 会话连接成功后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        protected sealed override void OnConnected(object sender, IContenxt context)
        {
            this.ApplyContextIdle(context);
        }

        /// <summary>
        /// SSL验证完成后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        protected sealed override void OnAuthenticated(object sender, IContenxt context)
        {
            this.ApplyContextIdle(context);
        }

        /// <summary>
        /// 收到请求后触发
        /// 此方法在先于协议中间件的Invoke方法调用
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        protected sealed override void OnRequested(object sender, IContenxt context)
        {
            this.ApplyContextIdle(context);
        }

        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="context">上下文</param>
        protected sealed override void OnDisconnected(object sender, IContenxt context)
        {
        }

        /// <summary>
        /// 服务异常事件
        /// </summary>
        /// <param name="sender">发生者</param>
        /// <param name="exception">异常</param>
        protected sealed override void OnException(object sender, Exception exception)
        {
        }

        /// <summary>
        /// 过滤上下文
        /// 返回true的上下文将被idle计时检测
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        protected virtual bool FilterContext(IContenxt context)
        {
            return true;
        }


        /// <summary>
        /// idle计时
        /// </summary>
        /// <param name="context">上下文</param>
        private void ApplyContextIdle(IContenxt context)
        {
            if (this.FilterContext(context) == true)
            {
                context.Session.Tag
                    .Get(IdleTimer, () => new Timer(this.OnIdleTimeout, context, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan))
                    .As<Timer>()
                    .Change(this.MaxIdleTime, Timeout.InfiniteTimeSpan);
            }
        }

        /// <summary>
        /// 会话空闲超时后
        /// </summary>
        /// <param name="contextState">上下文</param>
        private void OnIdleTimeout(object contextState)
        {
            var context = contextState as IContenxt;
            context.Session.Tag.Get(IdleTimer).As<Timer>().Dispose();
            context.Session.Close();
            context.StreamReader.Clear();
        }
    }
}
