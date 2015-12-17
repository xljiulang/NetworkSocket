using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http会话对象
    /// </summary>
    public class HttpSession : SessionBase
    {
        /// <summary>
        /// 获取或设置是否为Http事件会话
        /// </summary>
        public bool IsEventStream
        {
            get
            {
                return this.TagData.TryGet<bool>("IsEventStream");
            }
            set
            {
                this.TagData.Set("IsEventStream", value);
            }
        }

        /// <summary>
        /// 发送事件到客户端
        /// </summary>
        /// <param name="httpEvent">http事件</param>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool SendEvent(HttpEvent httpEvent)
        {
            if (this.IsEventStream == false)
            {
                throw new NotSupportedException("会话不支持HttpEvent ..");
            }

            if (httpEvent == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                var bytes = Encoding.UTF8.GetBytes(httpEvent.ToString());
                base.Send(bytes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
