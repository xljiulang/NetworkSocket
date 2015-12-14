using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastModels
{
    /// <summary>
    /// 表示登录结果
    /// </summary>
    [Serializable]
    public class LoginResult
    {
        public bool State { get; set; }

        public string Message { get; set; }
    }
}
