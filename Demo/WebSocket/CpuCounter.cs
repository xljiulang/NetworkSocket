using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket
{
    /// <summary>
    /// CPU性能检测
    /// </summary>
    public static class CpuCounter
    {
        /// <summary>
        /// 最近一次统计的值
        /// </summary>
        private static int lastValue = 0;

        /// <summary>
        /// 性能记数器
        /// </summary>
        private static PerformanceCounter counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        /// <summary>
        /// CPU时间变化事件
        /// </summary>
        public static event Action<int> CpuTimeChanged;

        /// <summary>
        /// CPU性能检测
        /// </summary>
        static CpuCounter()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    var value = (int)Math.Round(counter.NextValue());
                    if (value != lastValue && CpuTimeChanged != null)
                    {
                        CpuTimeChanged.Invoke(value);
                    }
                    lastValue = value;
                }
            });
        }
    }
}
