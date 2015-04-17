using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Json
{
    /// <summary>
    /// 表示服务行为异常
    /// </summary>
    [Serializable]
    public class ActionException : Exception
    {
        /// <summary>
        /// 获取服务行为上下文
        /// </summary>
        public ActionContext ActionContext { get; private set; }

        /// <summary>
        /// 服务行为异常
        /// </summary>
        /// <param name="actionContext">服务行为上下文</param>
        /// <param name="innerException">内部异常</param>
        public ActionException(ActionContext actionContext, Exception innerException)
            : base(innerException.Message, innerException)
        {
            this.ActionContext = actionContext;
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.InnerException.ToString();
        }
    }
}
