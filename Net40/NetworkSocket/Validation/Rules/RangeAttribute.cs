using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示验值要在一定的区间中
    /// 支持整型和双精度型验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RangeAttribute : ValidRuleBase
    {
        /// <summary>
        /// 获取或设置最小值
        /// </summary>
        protected double MinValue { get; set; }

        /// <summary>
        /// 获取或设置最大值
        /// </summary>
        protected double MaxValue { get; set; }
        
        /// <summary>
        /// 验值要在一定的区间中
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        public RangeAttribute(int minValue, int maxValue)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.ErrorMessage = "值要在区间[{0},{1}]内的整数";
        }

        /// <summary>
        /// 验值要在一定的区间中
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        public RangeAttribute(double minValue, double maxValue)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.ErrorMessage = "值要在区间[{0},{1}]内的数";
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

            double number = 0d;
            if (double.TryParse(value, out number))
            {
                return number >= this.MinValue && number <= this.MaxValue;
            }
            return false;
        }

        /// <summary>
        /// 获取错误提示信息
        /// </summary>     
        /// <returns></returns>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(this.ErrorMessage, this.MinValue, this.MaxValue);
        }
    }
}
