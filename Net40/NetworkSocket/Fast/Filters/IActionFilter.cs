using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Filters
{
    /// <summary>
    /// Action过滤器
    /// </summary>
    public interface IActionFilter : IFilter
    {
        /// <summary>
        /// 在执行服务行为前触发       
        /// </summary>
        /// <param name="actionContext">上下文</param>       
        /// <returns></returns>
        void OnExecuting(ActionContext actionContext);

        /// <summary>
        /// 在执行服务行为后触发
        /// </summary>
        /// <param name="actionContext">上下文</param>      
        void OnExecuted(ActionContext actionContext);
    }
}
