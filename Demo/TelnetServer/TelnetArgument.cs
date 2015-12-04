using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelnetServer
{
    /// <summary>
    /// 表示Telnet参数
    /// </summary>
    class TelnetArgument
    {
        /// <summary>
        /// 获取参数
        /// </summary>
        public string Argument { get;  private  set; }

        /// <summary>
        /// Telnet参数
        /// </summary>
        /// <param name="arg">参数</param>
        public TelnetArgument(string arg)
        {
            this.Argument = arg;
        }

        /// <summary>
        /// 获取多个参数
        /// </summary>
        /// <returns></returns>
        public string[] GetArguments()
        {
            if (this.Argument == null)
            {
                return new string[0];
            }
            return this.Argument.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 获取多个参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetArguments<T>()
        {
            return this.GetArguments()
                .Select(arg => (T)((IConvertible)arg).ToType(typeof(T), null))
                .ToArray();
        }


        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Argument;
        }
    }
}
