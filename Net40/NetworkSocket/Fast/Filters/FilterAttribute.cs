using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Filters
{
    /// <summary>
    /// 表示服务器服务方法过滤器基础特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class FilterAttribute : Attribute, IFilter
    {
        /// <summary>
        /// 缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, bool> multiuseAttributeCache = new ConcurrentDictionary<Type, bool>();

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
        /// 执行顺序
        /// 越小最优先
        /// </summary>
        public int Order { get; set; }

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
