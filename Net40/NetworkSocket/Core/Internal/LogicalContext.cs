using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示线程逻辑调用上下文
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LogicalContext<T>
    {
        /// <summary>
        /// 上下文名称
        /// </summary>
        private static readonly string contextName = "LogicalContext";

        /// <summary>
        /// 获取值
        /// </summary>
        /// <returns></returns>
        public T GetValue()
        {
            return (T)CallContext.LogicalGetData(contextName);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value">值</param>
        public void SetValue(T value)
        {
            CallContext.LogicalSetData(contextName, value);
        }

        /// <summary>
        /// 清除值
        /// </summary>
        public void FreeValue()
        {
            CallContext.FreeNamedDataSlot(contextName);
        }
    }
}
