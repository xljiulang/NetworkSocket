using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 提供协议分析
    /// </summary>
    internal static class Protocol
    {
        /// <summary>
        /// 空格
        /// </summary>
        private static readonly byte Space = 32;

        /// <summary>
        /// 双换行
        /// </summary>
        private static readonly byte[] DoubelCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");

        /// <summary>
        /// 支持的http方法
        /// </summary>
        private static readonly string[] MethodNames = Enum.GetNames(typeof(HttpMethod));

        /// <summary>
        /// 支持的http方法最大长度
        /// </summary>
        private static readonly int MedthodMaxLength = MethodNames.Max(m => m.Length);


        /// <summary>
        /// 是否为http协议
        /// </summary>
        /// <param name="buffer">收到的数据</param>
        /// <param name="headerLength">头数据长度，包括双换行</param>
        /// <returns></returns>
        public static bool IsHttp(IReceiveBuffer buffer, out int headerLength)
        {
            var methodLength = Protocol.GetMthodLength(buffer);
            var methodName = buffer.ReadString(methodLength, Encoding.ASCII);

            if (Protocol.MethodNames.Any(m => m.StartsWith(methodName, StringComparison.OrdinalIgnoreCase)) == false)
            {
                headerLength = 0;
                return false;
            }

            buffer.Position = 0;
            var headerIndex = buffer.IndexOf(Protocol.DoubelCrlf);
            if (headerIndex < 0)
            {
                headerLength = 0;
                return true;
            }

            headerLength = headerIndex + Protocol.DoubelCrlf.Length;
            return true;
        }

        /// <summary>
        /// 获取当前的http方法长度
        /// </summary>
        /// <param name="buffer">收到的数据</param>
        /// <returns></returns>
        private static int GetMthodLength(IReceiveBuffer buffer)
        {
            var maxLength = Math.Min(buffer.Length, Protocol.MedthodMaxLength + 1);
            for (var i = 0; i < maxLength; i++)
            {
                if (buffer[i] == Protocol.Space)
                {
                    return i - 1;
                }
            }
            return maxLength;
        }
    }
}
