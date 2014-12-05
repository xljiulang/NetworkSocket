using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Attributes
{
    /// <summary>
    /// 表示服务的实现者
    /// </summary>
    public enum Implements
    {
        /// <summary>
        /// 由自身实现
        /// </summary>
        Self,
        /// <summary>
        /// 由远程实现
        /// </summary>
        Remote
    }
}
