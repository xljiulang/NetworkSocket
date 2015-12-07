using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示全局过滤器
    /// </summary>
    public sealed class GlobalFilters : IEnumerable
    {
        /// <summary>
        /// 获取过过滤器过滤器
        /// </summary>
        private readonly List<IFilter> fiters = new List<IFilter>();

        /// <summary>
        /// 全局过滤器
        /// </summary>
        public GlobalFilters()
        {
        }

        /// <summary>
        /// 移除过滤器类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Remove<T>() where T : class, IFilter
        {
            for (var i = 0; i < this.fiters.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(this.fiters[i].GetType()) == true)
                {
                    this.fiters.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 添加过滤器并按Order字段排序
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public void Add(IFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException();
            }

            if (filter.AllowMultiple == false && this.fiters.Any(item => item.GetType() == filter.GetType()))
            {
                throw new ArgumentException(string.Format("类型为{0}过滤器不允许多个实例 ..", filter.GetType().Name));
            }

            this.fiters.Add(filter);
            this.fiters.Sort(new FilterComparer());
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return this.fiters.GetEnumerator();
        }

        /// <summary>
        /// 过滤器比较器
        /// </summary>
        private class FilterComparer : IComparer<IFilter>
        {
            /// <summary>
            /// 指示要比较的对象的相对顺序
            /// 值含义小于零x 小于 y。零x 等于 y。大于零x 大于 y
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(IFilter x, IFilter y)
            {
                if (x == null || y == null || x.Order == y.Order)
                {
                    return 0;
                }

                if (x.Order < y.Order)
                {
                    return -1;
                }
                return 1;
            }
        }
    }
}
