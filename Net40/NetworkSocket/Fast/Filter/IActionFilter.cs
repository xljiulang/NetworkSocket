using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Api行为过滤器
    /// </summary>
    public interface IActionFilter : IFilter
    {
        /// <summary>
        /// 在执行Api行为前触发       
        /// </summary>
        /// <param name="filterContext">上下文</param>       
        /// <returns></returns>
        void OnExecuting(ActionContext filterContext);

        /// <summary>
        /// 在执行Api行为后触发
        /// </summary>
        /// <param name="filterContext">上下文</param>      
        void OnExecuted(ActionContext filterContext);
    }
}
