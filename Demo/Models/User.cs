using ProtoBuf;
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
    [ProtoContract]
    public class User
    {
        /// <summary>
        /// 账号
        /// </summary>
        [ProtoMember(1)]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [ProtoMember(2)]
        public string Password { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [ProtoMember(3)]
        public string Name { get; set; }
    }
}
