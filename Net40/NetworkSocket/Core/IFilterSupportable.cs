using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 定义支持过滤器的接口
    /// </summary>
    public interface IFilterSupportable
    {
        /// <summary>
        /// 获取全局过滤器
        /// </summary>
        GlobalFilters GlobalFilter { get; }

        /// <summary>
        /// 获取或设置Api行为特性过滤器提供者
        /// </summary>
        IFilterAttributeProvider FilterAttributeProvider { get; set; }
    }
}
