using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkSocket.Flex
{
    /// <summary>
    /// 表示Silverlight通讯策略XML文件中间件
    /// </summary>
    public class SilverlightPolicyMiddleware : IMiddleware
    {
        /// <summary>
        /// 下一个中间件
        /// </summary>
        public IMiddleware Next { get; set; }

        /// <summary>
        /// 执行中间件          
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        Task IMiddleware.Invoke(IContenxt context)
        {
            if (context.Session.Protocol != null || context.Buffer.Length != 22)
            {
                return this.Next.Invoke(context);
            }

            context.Buffer.Position = 0;
            var request = context.Buffer.ReadString(Encoding.ASCII);
            if (string.Equals(request, "<policy-file-request/>", StringComparison.OrdinalIgnoreCase))
            {
                return new Task(() => this.SendPolicyXML(context));
            }
            return this.Next.Invoke(context);
        }

        /// <summary>
        /// 发送策略文件
        /// </summary>
        /// <param name="context">上下文</param>
        private void SendPolicyXML(IContenxt context)
        {
            try
            {
                var policyXml = this.GeneratePolicyXml();
                var bytes = Encoding.UTF8.GetBytes(policyXml);
                context.Session.Send(new ByteRange(bytes));
            }
            catch (Exception)
            {
            }
            finally
            {
                context.Session.Close();
            }
        }

        /// <summary>
        /// 生成策略xml
        /// </summary>
        /// <returns></returns>
        protected virtual string GeneratePolicyXml()
        {
            return new StringBuilder()
               .AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>")
               .AppendLine("<access-policy>")
               .AppendLine("<cross-domain-access>")
               .AppendLine("<policy>")
               .AppendLine("<allow-from>")
               .AppendLine("<domain uri=\"*\"/>")
               .AppendLine("</allow-from>")
               .AppendLine("<grant-to>")
               .AppendLine("<socket-resource port=\"4502-4534\" protocol=\"tcp\"/>")
               .AppendLine("</grant-to>")
               .AppendLine("</policy>")
               .AppendLine("</cross-domain-access>")
               .AppendLine("</access-policy>")
               .ToString();
        }
    }
}
