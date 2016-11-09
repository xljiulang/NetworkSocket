using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示Api行为的参数过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ParameterFilterAttribute : FilterAttribute
    {
        /// <summary>
        /// 获取参数索引
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Api行为的参数过滤器
        /// </summary>
        public ParameterFilterAttribute()
        {
            this.Order = -1;
        }

        /// <summary>
        /// 设置过滤器对应参数的索引
        /// </summary>
        /// <param name="index"></param>
        internal ParameterFilterAttribute SetWithIndex(int index)
        {
            this.Index = index;
            return this;
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnExecuting(IActionContext filterContext)
        {
        }

        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="filterContext"></param>
        protected sealed override void OnExecuted(IActionContext filterContext)
        {
        }

        /// <summary>
        /// 异常时
        /// </summary>
        /// <param name="filterContext"></param>
        protected sealed override void OnException(IExceptionContext filterContext)
        {
        }
    }
}
