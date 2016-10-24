using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http服务器事件
    /// </summary>
    public class HttpEvent
    {
        /// <summary>
        /// 获取或设置事件名称
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// 获取或设置数据内容
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 获取或设置id
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// 获取或设置下次尝试请求的时间间隔
        /// </summary>
        public TimeSpan Retry { get; set; }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var packet = new StringBuilder();
            if (this.Id != null)
            {
                packet.AppendFormat("id: {0}\n", this.Id);
            }
            if (string.IsNullOrEmpty(this.Event) == false)
            {
                packet.AppendFormat("event: {0}\n", this.Event);
            }
            if (this.Retry != TimeSpan.Zero)
            {
                packet.AppendFormat("retry: {0}\n", this.Retry.TotalMilliseconds);
            }

            var data = default(string);
            if (string.IsNullOrEmpty(this.Data) == true)
            {
                data = "data: \n\n";
            }
            else
            {
                data = string.Concat(this.Data.Replace("\r\n", "\n").Split('\n').Select(item => string.Format("data: {0}\n", item))) + "\n";
            }

            packet.Append(data);
            return packet.ToString();
        }
    }
}
