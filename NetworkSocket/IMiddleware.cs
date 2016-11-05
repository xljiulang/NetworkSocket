using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 定义中间件的行为
    /// </summary>
    public interface IMiddleware
    {
        /// <summary>
        /// 下一个中间件
        /// </summary>
        IMiddleware Next { set; }

        /// <summary>
        /// 当IListener收到会话的请求后
        /// 提供IContenxt并执行此方法
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        Task Invoke(IContenxt context);
    }
}
