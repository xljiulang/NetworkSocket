using Models;
using NetworkSocket;
using NetworkSocket.Fast;
using NetworkSocket.Fast.Attributes;
using Server.Attributes;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    /// <summary>
    /// 系统
    /// </summary>   
    public class SystemService : FastServiceBase
    {
        public IUserDao UserDao { get; set; }

        /// <summary>
        /// 获取服务组件版本号
        /// </summary>       
        /// <returns></returns>
        [Service(Implements.Self, 0)]
        [Log("获取版本号")]
        public string GetVersion()
        {
            return this.GetType().Assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// 登录操作
        /// </summary>       
        /// <param name="user">用户数据</param>
        /// <param name="ifAdmin"></param>
        /// <returns></returns>    
        [Service(Implements.Self, 100)]
        [Log("登录操作")]
        public bool Login(User user, bool ifAdmin)
        {
            // 调用客户端进行排序计算功能
            var sortResult = this.FastTcpServer
                .GetService<NotifyService>()
                .SortByClient(this.CurrentContext.Client, new List<int> { 3, 2, 1 })
                .Result;

            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            // 记录客户端的登录结果
            var state = this.UserDao.IsExist(user);           
            this.CurrentContext.Client.TagBag.Logined = state;
            return state;
        }
    }
}
