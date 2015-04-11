using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
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
        /// 每获取一次自增1
        /// </summary>
        /// <returns></returns>
        new public long GetHashCode()
        {
            lock (this.syncRoot)
            {
                return this.baseValue++;
            }
        }
    }
}
