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
        public static void NormalClose(this IClient<Response> client, CloseCodes code)
        {
            var content = ByteConverter.ToBytes((ushort)(code), Endians.Big);
            var response = new FrameResponse(Frames.Close, content);
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
            var response = new FrameResponse(Frames.Text, bytes);
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
            var response = new FrameResponse(Frames.Binary, content);
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
