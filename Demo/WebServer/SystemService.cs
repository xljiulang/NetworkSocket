using Models;
using NetworkSocket;
using NetworkSocket.WebSocket;
using NetworkSocket.WebSocket.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Filters;

namespace WebServer
{
    /// <summary>
    /// 系统
    /// </summary>   
    public class SystemService : JsonApiServiceBase
    {
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
        [Api]
        [LogFilter("登录操作")]
        public string Login(User user, bool ifAdmin)
        {           
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(user.Account) || string.IsNullOrEmpty(user.Password))
            {
                return "用户名和密码不能为空";
            }

            // 记录客户端的登录结果           
            this.CurrentContext.Client.TagBag.Logined = true;
            return "登录系统成功";
        }

    }
}
