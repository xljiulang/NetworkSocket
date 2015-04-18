using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示服务或Api行为过滤器基础特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class FilterAttribute : Attribute, IFilter
    {
        /// <summary>
        /// 排序
        /// </summary>
        private int order;

        /// <summary>
        /// 缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, bool> multiuseAttributeCache = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// 表示服务或Api行为过滤器基础特性
        /// </summary>
        public FilterAttribute()
        {
        }

        /// <summary>
        /// 表示服务或Api行为过滤器基础特性
        /// </summary>
        /// <param name="order">执行顺序 越小最优先</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public FilterAttribute(int order)
        {
            this.Order = order;
        }

        /// <summary>
        /// 获取特性是否允许多个实例
        /// </summary>
        /// <param name="attributeType">特性类型</param>
        /// <returns></returns>
        private static bool IsAllowMultiple(Type attributeType)
        {
            return multiuseAttributeCache.GetOrAdd(attributeType, type => type
                .GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                .Cast<AttributeUsageAttribute>()
                .First()
                .AllowMultiple);
        }

        /// <summary>
        /// 获取执行顺序
        /// 越小最优先
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("order", "order必须大于或等于0");
                }
                this.order = value;
            }
        }

        /// <summary>
        /// 获取是否允许多个实例存在
        /// </summary>
        public bool AllowMultiple
        {
            get
            {
                return IsAllowMultiple(this.GetType());
            }
        }
    }
}
