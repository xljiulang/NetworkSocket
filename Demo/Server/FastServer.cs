using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkSocket.Fast;
using NetworkSocket;
using NetworkSocket.Fast.Attributes;
using Models;


namespace Server
{
    /// <summary>
    /// FastServer服务 
    /// </summary>
    public class FastServer : FastTcpServerBase
    {
        /// <summary>
        /// 接收到客户端连接
        /// </summary>
        /// <param name="client">客户端</param>
        protected override void OnConnect(SocketAsync<FastPacket> client)
        {
            Console.WriteLine("客户端{0}连接进来，当前连接数为：{1}", client, this.AliveClients.Count);
        }

        /// <summary>
        /// 接收到客户端断开连接
        /// </summary>
        /// <param name="client">客户端</param>
        protected override void OnDisconnect(SocketAsync<FastPacket> client)
        {
            Console.WriteLine("客户端{0}断开连接，当前连接数为：{1}", client, this.AliveClients.Count);
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <param name="client"></param>
        /// <param name="exception"></param>
        protected override void OnException(SocketAsync<FastPacket> client, Exception exception)
        {
            Console.WriteLine(exception);
            base.OnException(client, exception);
        }


        /// <summary>
        /// 登录操作
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="user">用户数据</param>
        /// <param name="ifAdmin"></param>
        /// <returns></returns>
        [Service(Implements.Self, 100)]
        public bool Login(SocketAsync<FastPacket> client, User user, bool ifAdmin)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            Console.WriteLine("用户{0}登录操作...", user.Account);

            var dataBase = new List<User> { new User { Account = "abc", Password = "123456" }, new User { Account = "admin", Password = "123456" } };
            var state = dataBase.Exists(item => item.Account == user.Account && item.Password == user.Password);

            // 登录客户是否已验证通过
            client.TagBag.IsValidated = state;
            return state;
        }

        /// <summary>
        /// 求合操作
        /// 客户端登录并验证通过后才能调用此服务
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        [Login]
        [Service(Implements.Self, 101)]
        public int GetSun(SocketAsync<FastPacket> client, int x, int y, int z)
        {
            Console.WriteLine("收到GetSum({0}, {1}, {2})", x, y, z);
            return x + y + z;
        }

        /// <summary>
        /// 警告客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="title">标题</param>
        /// <param name="contents">信息内容</param>
        /// <returns></returns> 
        [Service(Implements.Remote, 102)]
        public void WarmingClient(SocketAsync<FastPacket> client, string title, string contents)
        {
            this.InvokeRemote(client, 102, title, contents);
        }

        /// <summary>
        /// 让客户端进行排序计算，并返回排序结果
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="list">要排序的数据</param>
        /// <param name="callBack">回调</param>
        [Service(Implements.Remote, 103)]
        public Task<List<int>> SortByClient(SocketAsync<FastPacket> client, List<int> list)
        {
            return this.InvokeRemote<List<int>>(client, 103, list);
        }
    }
}
