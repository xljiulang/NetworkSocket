using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAnnotation = System.ComponentModel.DataAnnotations;

namespace NetworkSocket.Validation.Rules
{
    /// <summary>
    /// 表示要求必须输入
    /// 此特性影响EF-CodeFirst生成的数据库字段为非空约束
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredAttribute : DataAnnotation.RequiredAttribute, IValidRule
    {
        /// <summary>
        /// 获取或设置排序索引
        /// 越小越优先
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 要求必须输入
        /// </summary>
        public RequiredAttribute()
        {
            this.OrderIndex = -1;
            this.ErrorMessage = "该项为必填项";
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
    }
}
