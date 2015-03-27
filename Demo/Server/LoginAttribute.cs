using NetworkSocket.Fast.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    /// <summary>
    /// 登录特性
    /// </summary>
    public class LoginAttribute : FilterAttribute
    {
        public override void OnExecuting(NetworkSocket.SocketAsync<NetworkSocket.Fast.FastPacket> client, NetworkSocket.Fast.FastPacket packet)
        {
            var valid = (bool)client.TagBag.IsValidated;
            if (valid == false)
            {
                throw new Exception("未登录就尝试请求其它服务");
            }
        }

        public override void OnExecuted(NetworkSocket.SocketAsync<NetworkSocket.Fast.FastPacket> client, NetworkSocket.Fast.FastPacket packet)
        {
        }
    }
}
