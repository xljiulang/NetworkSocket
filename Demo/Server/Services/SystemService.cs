using Models;
using NetworkSocket;
using NetworkSocket.Fast;
using Server.Filters;
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
    public class SystemService : FastApiService
    {
        public IUserDao UserDao { get; set; }

        /// <summary>
        /// 获取服务组件版本号
        /// </summary>       
        /// <returns></returns>
        [Api]
        [LogFilter("获取版本号")]
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
        [Api("System.Login")]
        [LogFilter("登录操作")]
        public bool Login(User user, bool ifAdmin)
        {          
            // 调用客户端的Sort
            var sortResult = this.CurrentContext.Session.InvokeApi<List<int>>("Sort", new List<int> { 3, 1, 2 }).Result;

            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            // 记录客户端的登录结果
            var state = this.UserDao.IsExist(user);
            this.CurrentContext.Session.TagBag.Logined = state;
            return state;
        }
    }
}
