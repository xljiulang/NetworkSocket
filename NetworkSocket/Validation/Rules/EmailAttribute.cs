using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示验证是邮箱格式
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EmailAttribute : ValidRuleBase
    {
        /// <summary>
        /// 验证是邮箱格式
        /// </summary>
        public EmailAttribute()
        {
            this.ErrorMessage = "请输入正确的邮箱";
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        protected override bool IsValid(string value, ValidContext validContext)
        {
            return Regex.IsMatch(value, @"^\w+(\.\w*)*@\w+\.\w+$");
        }
    }
}
