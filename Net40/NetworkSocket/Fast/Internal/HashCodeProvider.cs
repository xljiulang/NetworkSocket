using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast.Internal
{
    /// <summary>
    /// 数据包哈希码提供者
    /// </summary>
    internal class HashCodeProvider
    {
        /// <summary>
        /// 基准值
        /// </summary>
        private long baseValue = 0L;

        /// <summary>
        /// 同步锁
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public long GetPacketHashCode()
        {
            lock (this.syncRoot)
            {
                return this.baseValue++;
            }
        }
    }
}
