using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示特殊用途的命令
    /// 应用层不能使用这些命令
    /// </summary>
    public enum SpecialCommands : int
    {
        /// <summary>
        /// 服务组件版本号
        /// </summary>
        Version = int.MinValue ,
    }
}
