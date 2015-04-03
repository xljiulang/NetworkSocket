using Models;
using NetworkSocket;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    /// <summary>
    /// 客户端的实现
    /// </summary>
    public class RemoteServer : FastTcpClientBase
    {
        /// <summary>
        /// 唯一实例
        /// </summary>
        private static readonly Lazy<RemoteServer> instance = new Lazy<RemoteServer>(() => new RemoteServer());

        /// <summary>
        /// 获取唯一实例
        /// </summary>
        public static RemoteServer Instance
        {
            get
            {
                return instance.Value;
            }
        }
        /// <summary>
        /// 获取服务组件版本号
        /// </summary>       
        /// <returns></returns>
        [Service(Implements.Remote, 0)]
        public Task<string> GetVersion()
        {
            return this.InvokeRemote<string>(0);
        }

        [Service(Implements.Remote, 100)]
        public Task<Boolean> Login(User user, Boolean ifAdmin)
        {
            return this.InvokeRemote<Boolean>(100, user, ifAdmin);
        }

        [Service(Implements.Remote, 300)]
        public Task<Int32> GetSun(Int32 x, Int32 y, Int32 z)
        {
            return this.InvokeRemote<Int32>(300, x, y, z);
        }

        [Service(Implements.Self, 200)]
        public void WarmingClient(String title, String contents)
        {
        }

        [Service(Implements.Self, 201)]
        public List<Int32> SortByClient(List<Int32> list)
        {
            return list.OrderBy(item => item).ToList();
        }


        protected override void OnDisconnect()
        {
            this.ReConnect().ContinueWith(t => this.TryReConnect(t.Result));
        }

        private void TryReConnect(bool result)
        {
            if (result == false)
            {
                Thread.Sleep(1000);
                this.ReConnect().ContinueWith(t => this.TryReConnect(t.Result));
            }
        }
    }
}
