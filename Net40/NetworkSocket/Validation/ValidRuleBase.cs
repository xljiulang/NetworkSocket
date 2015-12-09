using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 表示验证规则特性基础类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ValidRuleBase : ValidationAttribute, IValidRule
    {
        /// <summary>
        /// 获取或设置排序索引
        /// 越小越优先
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 根据当前的验证特性来验证指定的值
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="validationContext">上下文</param>
        /// <returns></returns>
        protected sealed override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <returns></returns>
        public sealed override bool IsValid(object value)
        {
            return base.IsValid(value);
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        bool IValidRule.IsValid(object value, ValidContext validContext)
        {
            var stringValue = value == null ? null : value.ToString();
            if (string.IsNullOrWhiteSpace(stringValue) == true)
            {
                return true;
            }
            return this.IsValid(stringValue, validContext);
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        protected abstract bool IsValid(string value, ValidContext validContext);


        /// <summary>
        /// 格式化错误提示信息
        /// </summary>
        /// <param name="name">字段名字</param>
        /// <returns></returns>
        public override string FormatErrorMessage(string name)
        {
            return this.ErrorMessage;
        }
    }
}
