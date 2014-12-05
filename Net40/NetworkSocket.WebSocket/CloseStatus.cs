using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 关闭原因状态
    /// </summary>
    public enum CloseStatus : ushort
    {
        /// <summary>
        /// 表示没有原因
        /// </summary>
        Empty = 0,

        /// <summary>
        /// 正常关闭
        /// </summary>
        NormalClosure = 1000,
        /// <summary>
        /// 终端已离开
        /// </summary>
        GoingAway = 1001,
        /// <summary>
        /// 协议错误
        /// </summary>
        ProtocolError = 1002,
        /// <summary>
        /// 不支持的数据类型
        /// </summary>
        UnsupportedDataType = 1003,
        /// <summary>
        /// 预留
        /// </summary>
        NoStatusReceived = 1005,
        /// <summary>
        /// 异常关闭
        /// </summary>
        AbnormalClosure = 1006,
        /// <summary>
        /// 无效数据
        /// </summary>
        InvalidFramePayloadData = 1007,
        /// <summary>
        /// 策略错误
        /// </summary>
        PolicyViolation = 1008,
        /// <summary>
        /// 消息内容过长
        /// </summary>
        MessageTooBig = 1009,
        /// <summary>
        /// 委托扩展
        /// </summary>
        MandatoryExt = 1010,
        /// <summary>
        /// 服务器内部错误
        /// </summary>
        InternalServerError = 1011,
        /// <summary>
        /// 安全握手
        /// </summary>
        TLSHandshake = 1015
    }
}
