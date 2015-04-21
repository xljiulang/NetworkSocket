using Models;
using NetworkSocket;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
    /// <summary>
    /// 客户端的实现
    /// </summary>
    public class RemoteServer : FastTcpClient
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
        public Task<string> GetVersion()
        {
            return this.InvokeApi<string>("GetVersion");
        }

        public Task<Boolean> Login(User user, Boolean ifAdmin)
        {
            return this.InvokeApi<Boolean>("System.Login", user, ifAdmin);
        }

        public Task<Int32> GetSun(Int32 x, Int32 y, Int32 z)
        {
            return this.InvokeApi<Int32>("GetSun", x, y, z);
        }

        [Api]
        public void Warming(String title, String contents)
        {
        }

        [Api]
        public List<Int32> Sort(List<Int32> list)
        {
            return list.OrderBy(item => item).ToList();
        }

        // 循环重连
        protected override void OnDisconnect()
        {
            base.OnDisconnect();
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
