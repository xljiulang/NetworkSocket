using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示http分块回复
    /// </summary>
    public class HttpChunkedWirter : IDisposable
    {
        /// <summary>
        /// 是否已经输出http头
        /// </summary>
        private bool wirteHeader = false;

        /// <summary>
        /// 是否已输出结束
        /// </summary>
        private bool writeEnd = false;

        /// <summary>
        /// 回复对象
        /// </summary>
        private readonly HttpResponse response;

        /// <summary>
        /// 换行
        /// </summary>
        private static readonly byte[] CRLF = Encoding.ASCII.GetBytes("\r\n");

        /// <summary>
        /// http分块回复
        /// </summary>
        /// <param name="response">回复对象</param>
        internal HttpChunkedWirter(HttpResponse response)
        {
            this.response = response;
        }

        /// <summary>
        /// 输出内容块
        /// </summary>
        /// <param name="chucked">内容</param>   
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public bool Wirte(byte[] chucked)
        {
            if (chucked == null)
            {
                throw new ArgumentNullException();
            }
            if (chucked.Length == 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return this.WirteHeader() && this.WirteChunked(chucked, 0, chucked.Length);
        }

        /// <summary>
        /// 输出内容块
        /// </summary>
        /// <param name="chucked">内容</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public bool Wirte(ArraySegment<byte> chucked)
        {
            if (chucked == null)
            {
                throw new ArgumentNullException();
            }

            if (chucked.Count == 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return this.WirteHeader() && this.WirteChunked(chucked.Array, chucked.Offset, chucked.Count);
        }


        /// <summary>
        /// 结束输出内容块
        /// </summary>
        public bool End()
        {
            var state = this.WirteChunked(null, 0, 0);
            this.writeEnd = true;
            return state;
        }

        /// <summary>
        /// 输出http头
        /// </summary>
        private bool WirteHeader()
        {
            if (this.wirteHeader == true)
            {
                return true;
            }

            this.wirteHeader = true;
            this.response.Headers.Set("Transfer-Encoding", "chunked");
            return this.response.WriteHeader();
        }

        /// <summary>
        /// 输出内容块
        /// </summary>
        /// <param name="buffer">内容</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">长度</param>
        /// <returns></returns>
        private bool WirteChunked(byte[] buffer, int offset, int count)
        {
            if (this.writeEnd == true)
            {
                return false;
            }

            var builder = new ByteBuilder(Endians.Big);
            var size = Encoding.ASCII.GetBytes(Convert.ToString(count, 16));
            builder.Add(size);
            builder.Add(CRLF);
            if (buffer != null)
            {
                builder.Add(buffer, offset, count);
            }
            builder.Add(CRLF);
            if (count == 0)
            {
                builder.Add(CRLF);
            }
            return this.response.WriteContent(builder.ToArraySegment());
        }


        /// <summary>
        /// 结束输出内容块
        /// </summary>
        public void Dispose()
        {
            this.End();
        }
    }
}
