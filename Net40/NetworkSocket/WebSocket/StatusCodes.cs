using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 状态码
    /// </summary>
    public enum StatusCodes : ushort
    {
        /// <summary>
        /// 正常关闭
        /// 1000
        /// </summary>
        NormalClosure = 1000,

        /// <summary>
        /// 终端已离开
        /// 1001
        /// </summary>
        GoingAway = 1001,

        /// <summary>
        /// 协议错误
        /// 1002
        /// </summary>
        ProtocolError = 1002,

        /// <summary>
        /// 不支持的数据类型
        /// 1003
        /// </summary>
        UnsupportedDataType = 1003,

        /// <summary>
        /// 预留
        /// 1005
        /// </summary>
        NoStatusReceived = 1005,

        /// <summary>
        /// 异常关闭
        /// 1006
        /// </summary>
        AbnormalClosure = 1006,

        /// <summary>
        /// 无效数据
        /// 1007
        /// </summary>
        InvalidFramePayloadData = 1007,

        /// <summary>
        /// 策略错误
        /// 1008
        /// </summary>
        PolicyViolation = 1008,

        /// <summary>
        /// 消息内容过长
        /// 1009
        /// </summary>
        MessageTooBig = 1009,

        /// <summary>
        /// 委托扩展
        /// 1010
        /// </summary>
        MandatoryExt = 1010,

        /// <summary>
        /// 服务器内部错误
        /// 1011
        /// </summary>
        InternalServerError = 1011,

        /// <summary>
        /// 安全握手
        /// 1015
        /// </summary>
        TLSHandshake = 1015
    }
}
