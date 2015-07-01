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
    public class User
    {
        /// <summary>
        /// 账号
        /// </summary>       
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>       
        public string Password { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>        
        public string Name { get; set; }
    }
}
