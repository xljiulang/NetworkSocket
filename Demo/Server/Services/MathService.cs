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
    /// 数学计算
    /// 需要客户端登录才能访问
    /// </summary>
    [Login]
    public class MathService : FastServiceBase
    {
        /// <summary>
        /// 求合操作
        /// 客户端登录并验证通过后才能调用此服务
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        [Service(Implements.Self, 300)]
        [Log("日志")]
        public int GetSun(SocketAsync<FastPacket> client, int x, int y, int z)
        {
            Console.WriteLine("收到GetSum({0}, {1}, {2})", x, y, z);
            return x + y + z;
        }
    }
}
