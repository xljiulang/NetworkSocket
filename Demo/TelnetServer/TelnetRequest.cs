using NetworkSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelnetServer
{
    /// <summary>
    /// 表示Telnet请求
    /// </summary>
    class TelnetRequest
    {
        /// <summary>
        /// 换行符号
        /// </summary>
        private readonly static byte[] Crlf = Encoding.ASCII.GetBytes(Environment.NewLine);

        /// <summary>
        /// 从收到的数据流中解析请求体
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static TelnetRequest Parse(ReceiveStream stream)
        {
            stream.Position = 0;
            var index = stream.IndexOf(Crlf); // 以CRLF为结束标识
            if (index < 0)
            {
                return null;
            }
            var input = stream.ReadString(index, Encoding.ASCII);
            stream.Clear(index + Crlf.Length);

            if (string.IsNullOrWhiteSpace(input) == true) // 空内容
            {
                return TelnetRequest.Parse(stream);
            }

            var items = input.Split(new string[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
            var cmd = items.First();
            var arg = items.Length > 1 ? items.Last() : null;
            return new TelnetRequest(cmd, arg);
        }


        /// <summary>
        /// 获取命令
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// 获取参数
        /// </summary>
        public TelnetArgument Argument { get; private set; }    

        /// <summary>
        /// Telnet请求
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="arg">参数</param>
        public TelnetRequest(string cmd, string arg)
        {
            this.Command = cmd;
            this.Argument = new TelnetArgument (arg);
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Command;
        }
    }
}
