using Models;
using NetworkSocket;
using NetworkSocket.Core;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastClient
{
    /// <summary>
    /// 客户端
    /// 长连接单例模式
    /// </summary>
    public class Client : FastTcpClient
    {
        /// <summary>
        /// 唯一实例
        /// </summary>
        private static readonly Lazy<Client> instance = new Lazy<Client>(() => new Client());

        /// <summary>
        /// 获取唯一实例
        /// </summary>
        public static Client Instance
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

        /// <summary>
        /// 登录服务器
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <param name="ifAdmin">是否为管理员</param>
        /// <returns></returns>
        public Task<LoginResult> Login(UserInfo user, Boolean ifAdmin)
        {
            return this.InvokeApi<LoginResult>("Login", user, ifAdmin);
        }

        /// <summary>
        /// 求和运算
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Task<Int32> GetSum(Int32 x, Int32 y, Int32 z)
        {
            return this.InvokeApi<Int32>("GetSum", x, y, z);
        }


        /// <summary>
        /// 服务器的其它成员登录通知
        /// </summary>
        /// <param name="account">成员账号</param>
        [Api]
        public void LoginNotify(string account)
        {
            // 这里可以通过事件，让其它窗体来订阅
            MessageBox.Show("你的好友[" + account + "]登录了", "好友登录提示");
        }


        /// <summary>
        /// 浏览器器发来的通知
        /// </summary>
        /// <param name="message">消息内容</param>
        [Api]
        public void HttpNotify(string message)
        {
            MessageBox.Show(message, "系统提示");
        }
    }
}
