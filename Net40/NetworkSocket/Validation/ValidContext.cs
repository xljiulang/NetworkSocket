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
        public Property[] Properties { get; set; }
    }
}
