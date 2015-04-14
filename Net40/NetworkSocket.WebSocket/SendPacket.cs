using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示要回复客户端的封包
    /// </summary>
    public class SendPacket : Hybi13Packet
    {
        /// <summary>
        /// 设置握手数据
        /// </summary>
        /// <param name="handShake">握手数据</param>
        public void SetHandshake(byte[] handShake)
        {
            this.Bytes = handShake;
        }
        /// <summary>
        /// 设置二进制数据
        /// 数据将被进行帧处理
        /// </summary>
        /// <param name="frameType">帧类型</param>
        /// <param name="bytes">原始制数据</param>
        public void SetBody(FrameTypes frameType, byte[] bytes)
        {
            if (bytes == null)
            {
                bytes = new byte[0];
            }
            this.FrameType = frameType;
            this.Bytes = base.FrameBuffer(bytes, this.FrameType);
        }
    }
}
