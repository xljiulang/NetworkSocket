using NetworkSocket;
using NetworkSocket.Core;
using NetworkSocket.Fast;
using FastServer.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastServer.Services
{
    /// <summary>
    /// 数学计算服务  
    /// </summary>
    [LoginFilter] // 客户端登录才能访问
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
        [Api("GetSum")]
        [LogFilter("求合操作")]
        public int GetSun(int x, int y, int z)
        {
            return x + y + z;
        }
    }
}
