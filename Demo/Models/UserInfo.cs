using NetworkSocket.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    /// <summary>
    /// 用户信息实体
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// 账号
        /// </summary>       
        [Required(ErrorMessage = "账号为必填项")]
        [Length(10, MinimumLength = 5, ErrorMessage = "账号为{0}到{1}到字符")]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>       
        [Required(ErrorMessage = "密码为必填项")]
        public string Password { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>        
        public string Name { get; set; }
    }
}
