using NetworkSocket;
using NetworkSocket.WebSocket;
using NetworkSocket.WebSocket.Fast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket.Filters;

namespace WebSocket
{
    /// <summary>
    /// Cpu性能检测控制服务
    /// </summary>   
    public class CpuCounterService : FastApiService
    {
        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns></returns>
        [Api]
        [LogFilter("获取版本号")]
        public string GetVersion()
        {
            return this.GetType().Assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// 订阅/取消Cpu变化通知
        /// </summary>       
        /// <returns></returns>
        [Api]
        [LogFilter("订阅/取消Cpu变化通知")]
        public bool SubscribeCpuChangeNotify(bool subscribe)
        {
            this.CurrentContext.Session.TagData.Set("NotifyFlag", subscribe);
            return true;
        }
    }
}
