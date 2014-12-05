using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket;
using NetworkSocket.Fast;
using NetworkSocket.Fast.Attributes;

namespace ClientApp
{
    /// <summary>
    /// 客户端的实现
    /// </summary>
    public class FastServer : FastServerProxyBase
    {
        /// <summary>
        /// 客户端服务方法WarmingClient
        /// </summary>
        /// <param name="title"></param>
        /// <param name="contents"></param>
        public override void WarmingClient(string title, string contents)
        {
            Console.WriteLine(title);
            Console.WriteLine(contents);
        }


        /// <summary>
        /// 客户端服务方法SortByClient
        /// </summary>
        /// <param name="title"></param>
        /// <param name="contents"></param>
        public override List<int> SortByClient(List<int> list)
        {
            return list.OrderBy(item => item).ToList();
        }
    }
}
