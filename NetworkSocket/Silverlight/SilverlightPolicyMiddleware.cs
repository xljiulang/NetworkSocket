using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkSocket.Tasks;

namespace NetworkSocket.Silverlight
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
        async Task IMiddleware.Invoke(IContenxt context)
        {
            if (context.Session.Protocol != Protocol.None || context.StreamReader.Length != 22)
            {
                await this.Next.Invoke(context);
            }

            context.StreamReader.Position = 0;
            var request = context.StreamReader.ReadString(Encoding.ASCII);
            if (string.Equals(request, "<policy-file-request/>", StringComparison.OrdinalIgnoreCase))
            {
                this.SendPolicyXML(context);
            }
            else
            {
                await this.Next.Invoke(context);
            }
        }

        /// <summary>
        /// 发送策略文件
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        private bool SendPolicyXML(IContenxt context)
        {
            try
            {
                var policyXml = this.GeneratePolicyXml();
                var bytes = Encoding.UTF8.GetBytes(policyXml);
                return context.Session.Send(bytes) > 0;
            }
            catch (Exception)
            {
                return false;
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
