using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示SSE会话对象
    /// </summary>
    public class HttpEventSession : IWrapper
    {
        /// <summary>
        /// 会话对象
        /// </summary>
        private ISession session;

        /// <summary>
        /// 获取用户数据字典
        /// </summary>
        public ITag Tag
        {
            get
            {
                return this.session.Tag;
            }
        }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.session.RemoteEndPoint;
            }
        }

        /// <summary>
        /// 获取本机终结点
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                return this.session.LocalEndPoint;
            }
        }

        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.session.IsConnected;
            }
        }

        /// <summary>
        /// SSE会话对象
        /// </summary>
        /// <param name="session">会话对象</param>
        public HttpEventSession(ISession session)
        {
            this.session = session;
        }

        /// <summary>      
        /// 断开和远程端的连接
        /// </summary>
        public void Close()
        {
            this.session.Close();
        }

        /// <summary>
        /// 发送事件到客户端
        /// </summary>
        /// <param name="httpEvent">http事件</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool SendEvent(HttpEvent httpEvent)
        {
            if (httpEvent == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                var bytes = Encoding.UTF8.GetBytes(httpEvent.ToString());
                this.session.Send(bytes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 还原到包装前
        /// </summary>
        /// <returns></returns>
        public ISession UnWrap()
        {
            return this.session;
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.session.ToString();
        }
    }
}
