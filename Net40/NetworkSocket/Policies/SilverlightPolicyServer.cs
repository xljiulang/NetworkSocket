using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Policies
{
    /// <summary>
    /// Siverlight通讯策略服务   
    /// </summary>
    public class SilverlightPolicyServer : FlexPolicyServer
    {
        /// <summary>
        /// 获取策略服务端口
        /// 943
        /// </summary>
        public override int Port
        {
            get
            {
                return 943;
            }
        }

        /// <summary>
        /// 生成策略xml
        /// 返回null则不发送策略文件 
        /// </summary>
        /// <param name="input">请求内容</param>
        /// <returns></returns>
        protected override string GeneratePolicyXml(string input)
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
