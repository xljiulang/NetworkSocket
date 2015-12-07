using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 定义全局过滤器管理者
    /// 要求可枚举出过滤器实例
    /// </summary>
    public interface IGlobalFilters : IEnumerable
    {
        /// <summary>
        /// 添加过滤器并按Order字段排序
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>      
        void Add(IFilter filter);

        /// <summary>
        /// 移除某类型的过滤器实例
        /// </summary>
        /// <param name="filterType">过滤器的类型</param>
        void Remove(Type filterType);
    }
}
