using NetworkSocket.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http文件结果
    /// </summary>
    public class FileResult : ActionResult
    {
        /// <summary>
        /// 1M以下文件
        /// </summary>
        private static readonly long maxTxtFileSize = 1024L * 1024L;

        /// <summary>
        /// 文本文件格式
        /// </summary>
        private static readonly string[] txtFiles = new string[] { ".js", ".css", ".html", ".html", ".text", ".xml" };

        /// <summary>
        /// gizp支持的文件格式
        /// </summary>
        private static readonly HashSet<string> gzipHashSet = new HashSet<string>(txtFiles, StringComparer.OrdinalIgnoreCase);

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
            if (File.Exists(this.FileName) == true)
            {
                this.ExecuteFileResult(context);
            }
            else
            {
                var result = new ErrorResult { Status = 404, Errors = "找不到文件：" + this.FileName };
                result.ExecuteResult(context.Response);
            }
        } 

        /// <summary>
        /// 异步执行结果
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public async Task ExecuteResultAsync(RequestContext context)
        {
            if (File.Exists(this.FileName) == true)
            {
                await this.ExecuteFileResultAsync(context);
            }
            else
            {
                var result = new ErrorResult { Status = 404, Errors = "找不到文件：" + this.FileName };
                result.ExecuteResult(context.Response);
            }
        }

        /// <summary>
        /// 输出文件
        /// </summary>
        /// <param name="context">上下文</param>
        private void ExecuteFileResult(RequestContext context)
        {
            if (string.IsNullOrEmpty(this.ContentType))
            {
                this.ContentType = "application/ocelet-stream";
            }
            context.Response.Charset = null;
            context.Response.ContentType = this.ContentType;
            context.Response.ContentDisposition = this.ContentDisposition;

            using (var fileStream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read))
            {
                if (this.GZipSupported(context, fileStream) == true)
                {
                    this.ResponseFileByGzip(context.Response, fileStream);
                }
                else
                {
                    this.ResponseFile(context.Response, fileStream);
                }
            }
        }

        /// <summary>
        /// 输出文件
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private async Task ExecuteFileResultAsync(RequestContext context)
        {
            if (string.IsNullOrEmpty(this.ContentType))
            {
                this.ContentType = "application/ocelet-stream";
            }
            context.Response.Charset = null;
            context.Response.ContentType = this.ContentType;
            context.Response.ContentDisposition = this.ContentDisposition;

            using (var fileStream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read))
            {
                if (this.GZipSupported(context, fileStream) == true)
                {
                    await this.ResponseFileByGzipAsync(context.Response, fileStream);
                }
                else
                {
                    await this.ResponseFileAsync(context.Response, fileStream);
                }
            }
        }

        /// <summary>
        /// 文件是否能使用GZip
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="fileStream">文件流</param>
        /// <returns></returns>
        private bool GZipSupported(RequestContext context, FileStream fileStream)
        {
            if (context.Request.IsAcceptGZip() == false)
            {
                return false;
            }

            var ext = Path.GetExtension(this.FileName);
            if (FileResult.gzipHashSet.Contains(ext) == false)
            {
                return false;
            }

            return fileStream.Length < FileResult.maxTxtFileSize;
        }

        /// <summary>
        /// 输出文件
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileStream"></param>
        private void ResponseFile(HttpResponse response, FileStream fileStream)
        {
            const int size = 8 * 1024;
            var state = response.WriteHeader((int)fileStream.Length);
            var bytes = new byte[size];

            while (state == true)
            {
                var length = fileStream.Read(bytes, 0, size);
                if (length == 0)
                {
                    break;
                }
                var content = new ArraySegment<byte>(bytes, 0, length);
                state = response.WriteContent(content);
            }
        }

        /// <summary>
        /// 输出文件
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private async Task ResponseFileAsync(HttpResponse response, FileStream fileStream)
        {
            const int size = 8 * 1024;
            var state = response.WriteHeader((int)fileStream.Length);
            var bytes = new byte[size];

            while (state == true)
            {
                var length = await fileStream.ReadAsync(bytes, 0, size);
                if (length == 0)
                {
                    break;
                }
                var content = new ArraySegment<byte>(bytes, 0, length);
                state = response.WriteContent(content);
            }
        }

        /// <summary>
        /// 输出文件
        /// Gzip压缩
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileStream"></param>
        private void ResponseFileByGzip(HttpResponse response, FileStream fileStream)
        {
            var buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);

            var zipBuffer = Compression.GZipCompress(buffer);
            response.WriteHeader(zipBuffer.Length, true);
            response.WriteContent(zipBuffer);
        }

        /// <summary>
        /// 输出文件
        /// Gzip压缩
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private async Task ResponseFileByGzipAsync(HttpResponse response, FileStream fileStream)
        { 
            var buffer = new byte[fileStream.Length];
            await fileStream.ReadAsync(buffer, 0, buffer.Length);

            var zipBuffer = Compression.GZipCompress(buffer);
            response.WriteHeader(zipBuffer.Length, true);
            response.WriteContent(zipBuffer);
        }
    }
}
