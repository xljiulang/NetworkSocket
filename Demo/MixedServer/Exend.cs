using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixedServer
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class Exend
    {
        /// <summary>
        /// 调用远程端实现的服务方法        
        /// </summary>       
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param>   
        /// <returns></returns>
        public static bool TryInvokeApi(this FastWebSocketSession session, string api, params object[] parameters)
        {
            try
            {
                session.InvokeApi(api, parameters);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
