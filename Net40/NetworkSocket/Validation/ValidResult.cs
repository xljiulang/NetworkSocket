using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 表示验证结果
    /// </summary>
    public sealed class ValidResult
    {
        /// <summary>
        /// 获取验证状态
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 获取验证不通过属性名称
        /// </summary>
        public string ProperyName { get; set; }

        /// <summary>
        /// 获取验证不通过提示语
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
