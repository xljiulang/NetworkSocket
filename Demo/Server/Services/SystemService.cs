using Models;
using NetworkSocket;
using NetworkSocket.Fast;
using NetworkSocket.Fast.Attributes;
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
    public class SystemService : FastServiceBase
    {
        public IUserDao UserDao { get; set; }

        /// <summary>
        /// 获取服务组件版本号
        /// </summary>       
        /// <returns></returns>
        [Service(Implements.Self, 0)]
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
        [Service(Implements.Self, 100)]
        [LogFilter("登录操作")]
        public bool Login(User user, bool ifAdmin)
        {
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
