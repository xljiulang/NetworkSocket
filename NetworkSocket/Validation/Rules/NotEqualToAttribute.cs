using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示验证不要和目标ID的值一致
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NotEqualToAttribute : EqualToAttribute
    {
        /// <summary>
        /// 验证不要和目标属性的值一致
        /// </summary>       
        /// <param name="propertyName">目标属性</param>
        public NotEqualToAttribute(string propertyName)
            : base(propertyName)
        {
            this.ErrorMessage = "输入的内容不能重复";
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        protected override bool IsValid(string value, ValidContext validContext)
        {
            return !base.IsValid(value, validContext);
        }
    }
}
