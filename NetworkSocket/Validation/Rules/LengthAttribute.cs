using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DataAnnotation = System.ComponentModel.DataAnnotations;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示验证输入的长度范围
    /// maxLength参数会影响EF-CodeFirst生成的数据库字段最大长度
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LengthAttribute : DataAnnotation.StringLengthAttribute, IValidRule
    {
        /// <summary>
        /// 排序索引
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 验证输入的长度范围
        /// </summary>
        /// <param name="maxLength">最大长度</param>
        public LengthAttribute(int maxLength)
            : base(maxLength)
        {
            this.OrderIndex = 1;
            this.ErrorMessage = "长度必须介于{0}到{1}个字";
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        bool IValidRule.IsValid(object value, ValidContext validContext)
        {
            return base.IsValid(value);
        }

        /// <summary>
        /// 获取错误提示信息
        /// </summary>       
        /// <returns></returns>
        public override string FormatErrorMessage(string name)
        {
            var minLength = Math.Max(1, this.MinimumLength);
            return string.Format(this.ErrorMessage, minLength, this.MaximumLength);
        } 
    }
}
