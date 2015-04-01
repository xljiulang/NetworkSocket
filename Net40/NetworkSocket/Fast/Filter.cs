using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 过滤器
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// 获取或设置实例
        /// </summary>
        public IFilter Instance { get; set; }

        /// <summary>
        /// 获取或设置适用范围
        /// </summary>
        public FilterScope FilterScope { get; set; }
    }
}
