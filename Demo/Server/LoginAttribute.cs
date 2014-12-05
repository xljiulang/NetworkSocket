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
        public override bool OnExecuting(NetworkSocket.SocketAsync<NetworkSocket.Fast.FastPacket> client, NetworkSocket.Fast.FastPacket packet)
        {
            return client.TagBag.IsValidated;
        }
    }
}
