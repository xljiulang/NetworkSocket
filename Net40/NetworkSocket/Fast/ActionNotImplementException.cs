using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 服务行为未实现异常
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Message = {Message}")]
    public class ActionNotImplementException : Exception
    {
        /// <summary>
        /// 获取服务命令
        /// </summary>
        public long Command { get; private set; }

        /// <summary>
        /// 服务行为不支持异常
        /// </summary>
        /// <param name="command">服务命令</param>
        public ActionNotImplementException(long command)
            : base(string.Format("命令为{0}的服务行为未实现", command))
        {
            this.Command = command;
        }
    }
}
