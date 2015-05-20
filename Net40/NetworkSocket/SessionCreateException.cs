using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示创建会话异常
    /// </summary>
    public class SessionCreateException : Exception
    {
        /// <summary>
        /// 创建会话异常
        /// </summary>
        /// <param name="innerException">内部异常</param>
        public SessionCreateException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}
