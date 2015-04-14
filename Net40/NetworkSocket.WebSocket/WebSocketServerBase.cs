using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示WebSocket服务基础类
    /// 只支持 RFC 6455 协议
    /// </summary>    
    public abstract class WebSocketServerBase : TcpServerBase<SendPacket, RecvPacket>
    {
        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法      
        /// 返回的每一个数据包，将触发一次OnRecvComplete方法       
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="builder">接收到的历史数据</param>
        /// <returns></returns>
        protected override IEnumerable<RecvPacket> OnReceive(IClient<SendPacket> client, ByteBuilder builder)
        {
            RecvPacket packet;
            while ((packet = this.GetRecvPacket(client, builder)) != null)
            {
                yield return packet;
            }
        }

        /// <summary>
        /// 解析一个数据包
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="builder">接收到的历史数据</param>
        /// <returns></returns>
        private RecvPacket GetRecvPacket(IClient<SendPacket> client, ByteBuilder builder)
        {
            if (client.IsConnected == false)
            {
                return null;
            }

            // 获取处理接收的数据的容器
            var resultBuilder = client.TagBag.ResultBuilder as ByteBuilder;
            // 说明不是第一次握手请求
            if (resultBuilder != null)
            {
                return RecvPacket.From(builder, resultBuilder);
            }

            // 设置ResultBuilder            
            client.TagBag.ResultBuilder = new ByteBuilder();

            // 处理握手请求
            var request = HandshakeRequest.Parse(builder.ToArrayThenClear());
            if (request == null || this.CheckHandshake(client, request) == false)
            {
                this.CloseClient(client, CloseStatus.ProtocolError);
            }
            else
            {
                var packet = new SendPacket();
                packet.SetHandshake(request.ToHandshake());
                client.TrySend(packet);
            }

            return null;
        }


        /// <summary>
        /// 当收到到数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="tRecv">接收到的数据类型</param>
        protected override void OnRecvComplete(IClient<SendPacket> client, RecvPacket tRecv)
        {
            switch (tRecv.FrameType)
            {
                case FrameTypes.Close:
                    var reason = CloseStatus.Empty;
                    if (tRecv.Bytes.Length > 1)
                    {
                        var status = ByteConverter.ToUInt16(tRecv.Bytes, 0, Endians.Big);
                        if (Enum.IsDefined(typeof(CloseStatus), status))
                        {
                            reason = (CloseStatus)status;
                        }
                    }
                    this.OnClose(client, reason);
                    client.Close();
                    break;

                case FrameTypes.Binary:
                    this.OnBinary(client, tRecv.Bytes);
                    break;

                case FrameTypes.Text:
                    var text = Encoding.UTF8.GetString(tRecv.Bytes);
                    this.OnText(client, text);
                    break;

                case FrameTypes.Ping:
                    this.Send(client, FrameTypes.Pong, tRecv.Bytes);
                    this.OnPing(client, tRecv.Bytes);
                    break;

                case FrameTypes.Pong:
                    this.OnPong(client, tRecv.Bytes);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 向客户端发送关闭指令
        /// 断开客户端的连接并回收利用client对象
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="reason">关闭原因</param>
        public void CloseClient(IClient<SendPacket> client, CloseStatus reason)
        {
            var reasonByes = ByteConverter.ToBytes((ushort)(reason), Endians.Big);
            this.Send(client, FrameTypes.Close, reasonByes);
            client.Close();
        }

        /// <summary>
        /// 当收到握手请求时，将触发此方法
        /// 返回true说明验证握手请求参数合格
        /// 否则将安全关闭客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="request">握手请求</param>
        /// <returns></returns>
        protected virtual bool CheckHandshake(IClient<SendPacket> client, HandshakeRequest request)
        {
            return true;
        }


        /// <summary>
        /// 收到文本请求时触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="text">文本内容</param>
        protected abstract void OnText(IClient<SendPacket> client, string text);

        /// <summary>
        /// 收到二进制请求时触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="bytes">二进制内容</param>
        protected abstract void OnBinary(IClient<SendPacket> client, byte[] bytes);

        /// <summary>
        /// 收到Ping请求时触发此方法
        /// 在触发此方法之前，基础类已自动将pong回复此客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="bytes">二进制内容</param>
        protected virtual void OnPing(IClient<SendPacket> client, byte[] bytes)
        {
        }

        /// <summary>
        /// ping后客户端将回复pong触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="bytes">二进制内容</param>
        protected virtual void OnPong(IClient<SendPacket> client, byte[] bytes)
        {
        }

        /// <summary>
        /// 收到客户端关闭信息
        /// 在触发此方法后，基础类将自动安全关闭客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="reason">关闭原因</param>
        protected virtual void OnClose(IClient<SendPacket> client, CloseStatus reason)
        {
        }

        /// <summary>
        /// 发送给客户端文本内容
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="text">文本内容</param>
        protected void SendText(IClient<SendPacket> client, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            this.Send(client, FrameTypes.Text, bytes);
        }

        /// <summary>
        /// 发送给客户端二进制内容
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="bytes">二进制内容</param>
        protected void SendBinary(IClient<SendPacket> client, byte[] bytes)
        {
            this.Send(client, FrameTypes.Binary, bytes);
        }

        /// <summary>
        /// Ping客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="bytes">内容</param>
        protected void SendPing(IClient<SendPacket> client, byte[] bytes)
        {
            this.Send(client, FrameTypes.Ping, bytes);
        }

        /// <summary>
        /// 发送数据到客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="frameType">帧类型</param>
        /// <param name="bytes">内容</param>
        private void Send(IClient<SendPacket> client, FrameTypes frameType, byte[] bytes)
        {
            var packet = new SendPacket();
            packet.SetBody(frameType, bytes);
            client.Send(packet);
        }
    }
}
