using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示验证是否和目标ID的值一致
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class EqualToAttribute : ValidRuleBase
    {
        /// <summary>
        /// 目标属性的名称
        /// </summary>
        private string propertyName;

        /// <summary>
        /// 验证是否和目标属性的值一致
        /// </summary>        
        /// <param name="propertyName">目标属性</param>
        public EqualToAttribute(string propertyName)
        {
            this.propertyName = propertyName;
            this.ErrorMessage = "两次输入的字符不一至";
        } 

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        protected override bool IsValid(string value, ValidContext validContext)
        {
            var targetProperty = validContext.Properties.FirstOrDefault(item => item.Name == this.propertyName);
            if (targetProperty == null)
            {
                return false;
            }

            var tagrgetValue = targetProperty.GetValue(validContext.Instance);
            if (tagrgetValue == null)
            {
                return false;
            }
            return value == tagrgetValue.ToString();
        }
    }
}
