using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示验证是否和正则表达式匹配
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MatchAttribute : ValidRuleBase
    {
        /// <summary>
        /// 获取或设置正则表达式
        /// </summary>
        protected string RegexPattern { get; set; }

        /// <summary>
        /// 验证是否和正则表达式匹配
        /// </summary>
        /// <param name="pattern">正则表达式</param>
        public MatchAttribute(string pattern)
        {
            this.RegexPattern = pattern;
            this.OrderIndex = 1;
            this.ErrorMessage = "请输入正确的值";
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        protected override bool IsValid(string value, ValidContext validContext)
        {
            return Regex.IsMatch(value, this.RegexPattern);
        }
    }
}
