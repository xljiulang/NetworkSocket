using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using NetworkSocket.Util;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示WebSocket会话对象
    /// </summary>
    public class WebSocketSession : IWrapper
    {
        /// <summary>
        /// 会话对象
        /// </summary>
        private ISession session;

        /// <summary>
        /// 获取用户数据字典
        /// </summary>
        public ITag Tag
        {
            get
            {
                return this.session.Tag;
            }
        }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.session.RemoteEndPoint;
            }
        }

        /// <summary>
        /// 获取本机终结点
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                return this.session.LocalEndPoint;
            }
        }

        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.session.IsConnected;
            }
        }

        /// <summary>
        /// WebSocket会话对象
        /// </summary>
        /// <param name="session">会话</param>
        public WebSocketSession(ISession session)
        {
            this.session = session;
        }

        /// <summary>
        /// 发送回复数据
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ArgumentNullException"></exception>   
        /// <param name="response">回复内容</param>
        public void Send(WebsocketResponse response)
        {
            this.session.Send(response.ToByteRange());
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>     
        /// <param name="content">文本内容</param>
        /// <exception cref="SocketException"></exception>
        public void SendText(string content)
        {
            var bytes = content == null ? new byte[0] : Encoding.UTF8.GetBytes(content);
            var response = new FrameResponse(FrameCodes.Text, bytes);
            this.Send(response);
        }

        /// <summary>
        /// 发送二进制数据
        /// </summary>       
        /// <param name="content">二进制数据</param>
        /// <exception cref="SocketException"></exception>
        public void SendBinary(byte[] content)
        {
            var response = new FrameResponse(FrameCodes.Binary, content);
            this.Send(response);
        }


        /// <summary>
        /// 正常关闭客户端
        /// </summary>       
        /// <param name="code">关闭码</param>
        public void Close(StatusCodes code)
        {
            this.Close(code, string.Empty);
        }

        /// <summary>
        /// 正常关闭客户端
        /// </summary>      
        /// <param name="code">关闭码</param>
        /// <param name="reason">原因</param>
        public void Close(StatusCodes code, string reason)
        {
            var codeBytes = ByteConverter.ToBytes((ushort)(code), Endians.Big);
            var reasonBytes = Encoding.UTF8.GetBytes(reason ?? string.Empty);
            var content = new byte[codeBytes.Length + reasonBytes.Length];

            Array.Copy(codeBytes, 0, content, 0, codeBytes.Length);
            Array.Copy(reasonBytes, 0, content, codeBytes.Length, reasonBytes.Length);
            var response = new FrameResponse(FrameCodes.Close, content);

            try
            {
                this.Send(response);
            }
            catch (Exception)
            {
            }
            finally
            {
                this.session.Close();
            }
        }

        /// <summary>
        /// ping指令
        /// 远程将回复pong
        /// </summary>
        /// <param name="contents">内容</param>
        public void Ping(byte[] contents)
        {
            this.Send(new FrameResponse(FrameCodes.Ping, contents));
        }

        /// <summary>
        /// 还原到包装前
        /// </summary>
        /// <returns></returns>
        public ISession UnWrap()
        {
            return this.session;
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.session.ToString();
        }
    }
}
