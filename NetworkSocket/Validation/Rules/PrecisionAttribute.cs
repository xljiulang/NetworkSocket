using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示精度验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrecisionAttribute : ValidRuleBase
    {
        /// <summary>
        /// 获取或设置最小精度
        /// </summary>
        public int Min { get; private set; }

        /// <summary>
        /// 获取或设置最大精度
        /// </summary>
        public int Max { get; private set; }

        /// <summary>
        /// 表示精度验证        
        /// </summary>
        /// <param name="min">最小精度</param>
        /// <param name="max">最大精度</param>
        public PrecisionAttribute(int min, int max)
        {
            this.Min = min;
            this.Max = max;
            this.ErrorMessage = "精度为{0}到{1}位小数";
        }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        protected override bool IsValid(string value, ValidContext validContext)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            var values = value.Split('.');
            if (this.Max > 0 && values.Length > 0)
            {
                return values.Last().Length <= Max;
            }
            return true;
        }

        /// <summary>
        /// 获取错误提示信息
        /// </summary>     
        /// <returns></returns>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(this.ErrorMessage, this.Min, this.Max);
        }
    }
}
