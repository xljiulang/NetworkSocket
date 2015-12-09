using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http行为结果
    /// </summary>
    public abstract class ActionResult
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public abstract void ExecuteResult(RequestContext context);
    }
}
