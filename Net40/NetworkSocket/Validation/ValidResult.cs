using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public bool State { get; private set; }

        /// <summary>
        /// 获取验证不通过属性
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// 获取验证不通过提示语
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// 验证结果
        /// </summary>
        private ValidResult()
        {
        }

        /// <summary>
        /// 表示正确的验证结果
        /// </summary>
        /// <returns></returns>
        public static ValidResult True()
        {
            return new ValidResult { State = true };
        }

        /// <summary>
        /// 表示错误的验证结果
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public static ValidResult False(string message)
        {
            return new ValidResult { ErrorMessage = message };
        }

        /// <summary>
        /// 表示错误的验证结果
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="property">不通过的属性</param>
        /// <returns></returns>
        public static ValidResult False(string message, PropertyInfo property)
        {
            return new ValidResult { ErrorMessage = message, Property = property };
        }
    }
}
