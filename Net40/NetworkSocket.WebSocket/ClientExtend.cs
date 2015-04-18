using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 客户端对象扩展
    /// </summary>
    public static class ClientExtend
    {
        /// <summary>
        /// 正常关闭客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="code">关闭码</param>
        public static void NormalClose(this IClient<Response> client, StatusCodes code)
        {
            client.NormalClose(code, string.Empty);
        }

        /// <summary>
        /// 正常关闭客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="code">关闭码</param>
        /// <param name="reason">原因</param>
        public static void NormalClose(this IClient<Response> client, StatusCodes code, string reason)
        {
            var codeBytes = ByteConverter.ToBytes((ushort)(code), Endians.Big);
            var reasonBytes = Encoding.UTF8.GetBytes(reason ?? string.Empty);
            var content = new byte[codeBytes.Length + reasonBytes.Length];

            Array.Copy(codeBytes, 0, content, 0, codeBytes.Length);
            Array.Copy(reasonBytes, 0, content, codeBytes.Length, reasonBytes.Length);

            var response = new FrameResponse(FrameCodes.Close, content);
            client.TrySend(response);
            client.Close();
        }


        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">文本内容</param>
        /// <exception cref="SocketException"></exception>
        public static void Send(this IClient<Response> client, string content)
        {
            var bytes = content == null ? new byte[0] : Encoding.UTF8.GetBytes(content);
            var response = new FrameResponse(FrameCodes.Text, bytes);
            client.Send(response);
        }

        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">二进制数据</param>
        /// <exception cref="SocketException"></exception>
        public static void Send(this IClient<Response> client, byte[] content)
        {
            var response = new FrameResponse(FrameCodes.Binary, content);
            client.Send(response);
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">文本内容</param>
        public static bool TrySend(this IClient<Response> client, string content)
        {
            try
            {
                client.Send(content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="content">二进制数据</param>     
        public static bool TrySend(this IClient<Response> client, byte[] content)
        {
            try
            {
                client.Send(content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
