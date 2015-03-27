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
        /// 代理代码命令
        /// </summary>
        ProxyCode = int.MinValue,
    }
}
