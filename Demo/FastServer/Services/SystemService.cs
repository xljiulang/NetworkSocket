using Models;
using NetworkSocket;
using NetworkSocket.Core;
using NetworkSocket.Fast;
using FastServer.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.Validation;
using System.Threading;

namespace FastServer.Services
{
    /// <summary>
    /// 系统登录服务
    /// </summary>   
    public class SystemService : FastApiService
    {
        /// <summary>
        /// 获取其它已登录的会话
        /// </summary>
        public IEnumerable<FastSession> OtherSessions
        {
            get
            {
                return this
                    .CurrentContext
                    .AllSessions
                    .Where(item => item.TagData.TryGet<bool>("Logined"))
                    .Where(item => item != this.CurrentContext.Session);
            }
        }

        /// <summary>
        /// 获取服务组件版本号
        /// </summary>       
        /// <returns></returns>
        [Api]
        [LogFilter("获取版本号")]
        public string GetVersion()
        {
            return typeof(FastApiService).Assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// 登录操作
        /// </summary>       
        /// <param name="user">用户数据</param>
        /// <param name="ifAdmin"></param>
        /// <returns></returns>    
        [Api]
        [LogFilter("登录操作")]
        public LoginResult Login(UserInfo user, bool ifAdmin)
        {
            var validResult = Model.ValidFor(user);
            if (validResult.State == false)
            {
                return new LoginResult { Message = validResult.ErrorMessage };
            }

            // 通知其它传话有新成员登录
            foreach (var session in this.OtherSessions)
            {
                session.InvokeApi("LoginNotify", user.Account);
            }

            // 标记会话已登录成功
            this.CurrentContext.Session.TagBag.Logined = true;
            return new LoginResult { State = true };
        }
    }
}
