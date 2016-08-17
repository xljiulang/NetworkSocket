using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 表示验证上下文
    /// 不可继承
    /// </summary>
    public sealed class ValidContext
    {
        /// <summary>
        /// 获取或设置模型的实例
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// 获取或设置实例类型的属性
        /// </summary>
        public RuleProperty[] Properties { get; set; }

        /// <summary>
        /// 验证上下文
        /// </summary>
        public ValidContext()
        {
        }

        /// <summary>
        /// 验证上下文
        /// </summary>
        /// <param name="instance">模型的实例</param>
        /// <param name="properties">实例类型的属性</param>
        public ValidContext(object instance, RuleProperty[] properties)
        {
            this.Instance = instance;
            this.Properties = properties;
        }
    }
}
