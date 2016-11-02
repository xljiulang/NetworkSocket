using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 表示WebSocket关闭请求帧
    /// </summary>
    public sealed class CloseRequest : FrameRequest
    {
        /// <summary>
        /// 请求帧
        /// </summary>
        private readonly FrameRequest request;

        /// <summary>
        /// 获取请求帧的内容
        /// </summary>
        public override byte[] Content
        {
            get
            {
                return this.request.Content;
            }
        } 


        /// <summary>
        /// 获取内容长度
        /// </summary>
        public override int ContentLength
        {
            get
            {
                return this.request.ContentLength;
            }
        } 

        /// <summary>
        /// 获取是否已完成
        /// </summary>
        public override bool Fin
        {
            get
            {
                return this.request.Fin;
            }
        }

        /// <summary>
        /// 获取帧类型
        /// </summary>
        public override FrameCodes Frame
        {
            get
            {
                return this.request.Frame;
            }
        }

        /// <summary>
        /// 获取是否有掩码
        /// </summary>
        public override bool Mask
        {
            get
            {
                return this.request.Mask;
            }
        }

        /// <summary>
        /// 获取掩码
        /// </summary>
        public override byte[] MaskingKey
        {
            get
            {
                return this.request.MaskingKey;
            }
        }

        /// <summary>
        ///  获取保存位
        /// </summary>
        public override ByteBits Rsv
        {
            get
            {
                return this.request.Rsv;
            }
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public StatusCodes StatusCode
        {
            get
            {
                if (this.Content.Length > 1)
                {
                    return (StatusCodes)ByteConverter.ToUInt16(this.Content, 0, Endians.Big);
                }
                return StatusCodes.NormalClosure;
            }
        }

        /// <summary>
        /// 备注原因
        /// </summary>
        public string CloseReason
        {
            get
            {
                if (this.Content.Length > 1)
                {

                    return Encoding.UTF8.GetString(this.Content, 2, this.Content.Length - 2);
                }
                return null;
            }
        }

        /// <summary>
        /// 关闭请求帧
        /// </summary>
        /// <param name="request">请求帧</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public CloseRequest(FrameRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException();
            }
            if (request.Frame != FrameCodes.Close)
            {
                throw new ArgumentException();
            }

            this.request = request;
        }
    }
}
