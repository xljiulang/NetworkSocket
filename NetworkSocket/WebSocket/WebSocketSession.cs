using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using NetworkSocket.Util;
using NetworkSocket.Tasks;
using System.Threading.Tasks;

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
        private readonly ISession session;

        /// <summary>
        /// ping任务表
        /// </summary>
        private readonly TaskSetterTable<Guid> pingTable = new TaskSetterTable<Guid>();

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
            this.session.Subscribe("Pong", this.OnPong);
        }

        /// <summary>
        /// 收到Pong时
        /// </summary>
        /// <param name="data">数据</param>
        private void OnPong(object data)
        {
            var value = data as byte[];
            if (value == null || value.Length != 36)
            {
                return;
            }

            Guid id;
            var valueString = Encoding.UTF8.GetString(value);
            if (Guid.TryParse(valueString, out id) == false)
            {
                return;
            }

            var setter = this.pingTable.Take(id);
            if (setter != null)
            {
                setter.SetResult(true);
            }
        }

        /// <summary>
        /// 尝试发送回复数据
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ArgumentNullException"></exception>   
        /// <param name="response">回复内容</param>
        public bool TrySend(WebsocketResponse response)
        {
            try
            {
                this.Send(response);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 发送回复数据
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ArgumentNullException"></exception>   
        /// <param name="response">回复内容</param>
        public int Send(WebsocketResponse response)
        {
            return this.session.Send(response.ToArraySegment(mask: false));
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>     
        /// <param name="content">文本内容</param>
        /// <exception cref="SocketException"></exception>
        public int SendText(string content)
        {
            var bytes = content == null ? new byte[0] : Encoding.UTF8.GetBytes(content);
            var text = new FrameResponse(FrameCodes.Text, bytes);
            return this.Send(text);
        }

        /// <summary>
        /// 发送二进制数据
        /// </summary>       
        /// <param name="content">二进制数据</param>
        /// <exception cref="SocketException"></exception>
        public int SendBinary(byte[] content)
        {
            var bin = new FrameResponse(FrameCodes.Binary, content);
            return this.Send(bin);
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
            var response = new CloseResponse(code, reason);
            this.TrySend(response);
            this.session.Close();
        }

        /// <summary>
        /// 向客户端ping唯一的内容
        /// 并等待匹配的回复
        /// </summary>
        /// <param name="waitTime">最多等待时间，超时则结果false</param>
        /// <returns></returns>
        public async Task<bool> PingAsync(TimeSpan waitTime)
        {
            var id = Guid.NewGuid();
            var task = this.pingTable.Create<bool>(id, waitTime);

            try
            {
                var content = Encoding.UTF8.GetBytes(id.ToString());
                var ping = new FrameResponse(FrameCodes.Ping, content);
                this.TrySend(ping);
                return await task;
            }
            catch (Exception)
            {
                return false;
            }
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
