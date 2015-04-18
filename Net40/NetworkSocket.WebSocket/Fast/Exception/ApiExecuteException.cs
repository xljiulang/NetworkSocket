using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// 表示Api执行异常
    /// </summary>
    [Serializable]
    public class ApiExecuteException : Exception
    {
        /// <summary>
        /// 获取Api行为上下文
        /// </summary>
        public ActionContext ActionContext { get; private set; }

        /// <summary>
        /// Api执行异常
        /// </summary>
        /// <param name="actionContext">Api行为上下文</param>
        /// <param name="innerException">内部异常</param>
        public ApiExecuteException(ActionContext actionContext, Exception innerException)
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
