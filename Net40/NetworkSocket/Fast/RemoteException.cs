using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示远程端服务行为异常
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Message = {Message}")]
    public class RemoteException : Exception
    {
        /// <summary>
        /// 获取或设置命令
        /// </summary>
        public int Command { get; set; }

        /// <summary>
        /// 获取或设置异常原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 远程端服务行为异常
        /// </summary>
        public RemoteException()
        {
        }

        /// <summary>
        /// 远程端服务行为异常
        /// </summary>
        /// <param name="cmd">请求指令</param>
        public RemoteException(int cmd)
            : this(cmd, null)
        {
        }

        /// <summary>
        /// 远程端服务行为异常
        /// </summary>
        /// <param name="cmd">请求指令</param>
        /// <param name="reason">异常原因</param>
        public RemoteException(int cmd, string reason)
        {
            this.Command = cmd;
            this.Reason = reason;
        }

        /// <summary>
        /// 获取提示消息
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("远程端命令为{0}的服务行为异常", this.Command);
            }
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Reason;
        }
    }
}
