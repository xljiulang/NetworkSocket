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
        /// 支持的http方法
        /// </summary>
        private static readonly string[] MethodNames = Enum.GetNames(typeof(HttpMethod));

        /// <summary>
        /// 支持的http方法最大长度
        /// </summary>
        private static readonly int MedthodMaxLength = MethodNames.Max(m => m.Length);

        /// <summary>
        /// 获取双换行
        /// </summary>
        public static readonly byte[] DoubleCrlf = Encoding.ASCII.GetBytes("\r\n\r\n");


        /// <summary>
        /// 是否为http协议
        /// </summary>
        /// <param name="stream">收到的数据</param>
        /// <param name="headerLength">头数据长度，包括双换行</param>
        /// <returns></returns>
        public static bool IsHttp(INsStream stream, out int headerLength)
        {
            var methodLength = Protocol.GetMthodLength(stream);
            var methodName = stream.ReadString(methodLength, Encoding.ASCII);

            if (Protocol.MethodNames.Any(m => m.StartsWith(methodName, StringComparison.OrdinalIgnoreCase)) == false)
            {
                headerLength = 0;
                return false;
            }

            stream.Position = 0;
            var headerIndex = stream.IndexOf(Protocol.DoubleCrlf);
            if (headerIndex < 0)
            {
                headerLength = 0;
                return true;
            }

            headerLength = headerIndex + Protocol.DoubleCrlf.Length;
            return true;
        }

        /// <summary>
        /// 获取当前的http方法长度
        /// </summary>
        /// <param name="stream">收到的数据</param>
        /// <returns></returns>
        private static int GetMthodLength(INsStream stream)
        {
            var maxLength = Math.Min(stream.Length, Protocol.MedthodMaxLength + 1);
            for (var i = 0; i < maxLength; i++)
            {
                if (stream[i] == Protocol.Space)
                {
                    return i - 1;
                }
            }
            return maxLength;
        }
    }
}
