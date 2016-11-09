using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示Api行为的参数值不能为NULL验证标记
    /// </summary>
    public class NotNullAttribute : ParameterFilterAttribute
    {
        /// <summary>
        /// 执行请求前
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnExecuting(IActionContext filterContext)
        {
            var value = filterContext.Action.ParameterValues[this.Index];
            if (value == null)
            {
                var paramterName = filterContext.Action.ParameterInfos[this.Index].Name;
                throw new ArgumentNullException(paramterName);
            }
        }
    }
}
