using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 关闭回复
    /// </summary>
    public sealed class CloseResponse : FrameResponse
    {
        /// <summary>
        /// 正常关闭客户端
        /// </summary>      
        /// <param name="code">关闭码</param>
        /// <param name="reason">原因</param>
        public CloseResponse(StatusCodes code, string reason)
            : base(FrameCodes.Close, GetContent(code, reason))
        {
        }

        /// <summary>
        /// 获取内容
        /// </summary>
        /// <param name="code">关闭码</param>
        /// <param name="reason">原因</param>
        /// <returns></returns>
        private static byte[] GetContent(StatusCodes code, string reason)
        {
            var codeBytes = ByteConverter.ToBytes((ushort)(code), Endians.Big);
            var reasonBytes = Encoding.UTF8.GetBytes(reason ?? string.Empty);
            var content = new byte[codeBytes.Length + reasonBytes.Length];

            Array.Copy(codeBytes, 0, content, 0, codeBytes.Length);
            Array.Copy(reasonBytes, 0, content, codeBytes.Length, reasonBytes.Length);
            return content;
        }
    }
}
