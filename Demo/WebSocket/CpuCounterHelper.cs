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
    public static class CpuCounterHelper
    {
        /// <summary>
        /// 性能记数器
        /// </summary>
        private static PerformanceCounter counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        /// <summary>
        /// CPU时间变化事件
        /// </summary>
        public static event Action<int> OnCpuTimeChanged;

        /// <summary>
        /// CPU性能检测
        /// </summary>
        static CpuCounterHelper()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    var value = (int)Math.Round(counter.NextValue());
                    if (OnCpuTimeChanged != null)
                    {
                        OnCpuTimeChanged(value);
                    }
                }
            });
        }
    }
}
