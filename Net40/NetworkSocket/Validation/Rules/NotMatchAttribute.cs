using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示验证不要和正则表达式匹配
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NotMatchAttribute : MatchAttribute
    {
        /// <summary>
        /// 验证不要和正则表达式匹配
        /// </summary>
        /// <param name="pattern">正则表达式</param>
        public NotMatchAttribute(string pattern)
            : base(pattern)
        {
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        protected override bool IsValid(string value,ValidContext validContext)
        {
            return !base.IsValid(value, validContext);
        }
    }
}
