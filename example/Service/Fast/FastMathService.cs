using Models;
using NetworkSocket;
using NetworkSocket.Core;
using NetworkSocket.Fast;
using NetworkSocket.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Fast
{
    /// <summary>
    /// fast协议Api服务  
    /// </summary>
    public class FastMathService : FastApiService
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
                    .FastSessions
                    .Where(item => item != this.CurrentContext.Session);
            }
        }

        /// <summary>
        /// 获取服务组件版本号
        /// </summary>       
        /// <returns></returns>
        [Api]
        [FastLogFilter("获取版本号")]
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
        [FastLogFilter("登录操作")]
        public LoginResult Login(UserInfo user, bool ifAdmin)
        {
            var validResult = Model.ValidFor(user);
            if (validResult.State == false)
            {
                return new LoginResult { Message = validResult.ErrorMessage };
            }

            // 通知其它fast会话有新成员登录
            foreach (var session in this.OtherSessions)
            {
                session.InvokeApi("LoginNotify", user.Account);
            }

            CurrentContext.Session.Tag.Set("Logined", true);
            return new LoginResult { State = true };
        }

        /// <summary>
        /// 求合操作       
        /// </summary>     
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        [Api("GetSum")]
        [FastLogFilter("求合操作")]
        [FastLogin]
        public int GetSun(int x, int y, int z)
        {
            return x + y + z;
        }
    }
}
