using NetworkSocket;
using NetworkSocket.Fast;
using Server.Filters;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Services
{
    /// <summary>
    /// 数学计算
    /// 需要客户端登录才能访问
    /// </summary>
    [LoginFilter]
    public class MathService : FastApiService
    {
        /// <summary>
        /// 求合操作
        /// 客户端登录并验证通过后才能调用此服务
        /// </summary>     
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        [Api]
        [LogFilter("求合操作")]
        public int GetSun(int x, int y, int z)
        {
            // 模拟长时间运算           
            return x + y + z;
        }
    }
}
