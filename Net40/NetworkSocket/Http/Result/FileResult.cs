using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示文件结果
    /// </summary>
    public class FileResult : ActionResult
    {
        /// <summary>
        /// 获取或设置文件路径
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 获取或设置内容类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 获取或设置内容描述
        /// </summary>
        public string ContentDisposition { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(RequestContext context)
        {
            context.Response.Charset = null;
            context.Response.ContentType = this.ContentType;
            context.Response.ContentDisposition = this.ContentDisposition;

            using (var stream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read))
            {
                const int size = 8 * 1024;
                var state = context.Response.WriteHeader((int)stream.Length);

                while (state == true)
                {
                    var bytes = new byte[size];
                    var length = stream.Read(bytes, 0, size);
                    if (length == 0)
                    {
                        break;
                    }

                    var content = new ByteRange(bytes, 0, length);
                    state = context.Response.WriteContent(content);
                }
            }
        }
    }
}
