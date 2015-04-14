using NetworkSocket;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    /// <summary>
    /// 通知服务
    /// </summary>
    public class NotifyService : FastServiceBase
    {
        /// <summary>
        /// 警告客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="title">标题</param>
        /// <param name="contents">信息内容</param>
        /// <returns></returns> 
        [Service(Implements.Remote, 200)]
        public Task WarmingClient(IClient<FastPacket> client, string title, string contents)
        {
            return this.InvokeRemote(client, 200, title, contents);
        }

        /// <summary>
        /// 让客户端进行排序计算，并返回排序结果
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="list">要排序的数据</param>
        /// <param name="callBack">回调</param>
        [Service(Implements.Remote, 201)]
        public Task<List<int>> SortByClient(IClient<FastPacket> client, List<int> list)
        {
            return this.InvokeRemote<List<int>>(client, 201, list);
        }
    }
}
