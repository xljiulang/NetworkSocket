using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Attributes
{
    /// <summary>
    /// 登录特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LoginAttribute : FilterAttribute, IAuthorizationFilter
    {       
        public void OnAuthorization(NetworkSocket.SocketAsync<NetworkSocket.Fast.FastPacket> client, NetworkSocket.Fast.FastPacket packet)
        {
            bool valid = client.TagBag.IsValidated ?? false;
            if (valid == false)
            {
                throw new Exception("未登录就尝试请求其它服务");
            }          
        }
    }
}
