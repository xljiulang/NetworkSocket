using Models;
using NetworkSocket.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixServer.AppStart
{
    public static partial class Config
    {
        public static void ConfigValidation()
        {
            Model.Fluent<UserInfo>()
                .Required(item => item.Account, "账号不能为空")
                .Length(item => item.Account, 10, 5, "账号为{0}到{1}个字符")
                .Required(item => item.Password, "密码不能为空")
                .Length(item => item.Password, 12, 6, "密码为{0}到{1}个字符");
        }
    }
}
