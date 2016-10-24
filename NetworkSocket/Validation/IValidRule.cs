using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 定义属性的验证规则行为的接口
    /// </summary>
    public interface IValidRule
    {
        /// <summary>
        /// 获取或设置排序索引
        /// 越小越优先
        /// </summary>
        int OrderIndex { get; set; }

        /// <summary>
        /// 验证属性的值是否通过
        /// </summary>
        /// <param name="value">属性的值</param>
        /// <param name="validContext">验证上下文</param>
        /// <returns></returns>
        bool IsValid(object value, ValidContext validContext);

        /// <summary>
        /// 格式化错误信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string FormatErrorMessage(string name);
    }
}
