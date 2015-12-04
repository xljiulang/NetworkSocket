using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelnetServer
{
    /// <summary>
    /// 表示Telnet会话
    /// </summary>
    class TelnetSession : NetworkSocket.SessionBase
    {
        /// <summary>
        /// 发送文本到Telnet客户端
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool Send(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text + Environment.NewLine);
            try
            {
                base.Send(bytes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
