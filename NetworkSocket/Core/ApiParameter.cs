using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示Api参数
    /// </summary>
    public sealed class ApiParameter
    {
        /// <summary>
        /// 线程本地保存的参数值 
        /// </summary>
        private readonly ThreadLocal<object> localValue = new ThreadLocal<object>();

        /// <summary>
        /// 获取参数信息
        /// </summary>
        public ParameterInfo Info { get; private set; }

        /// <summary>
        /// 获取参数索引
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// 获取参数类型
        /// </summary>
        public Type Type
        {
            get
            {
                return this.Info.ParameterType;
            }
        }

        /// <summary>
        /// 获取参数名
        /// </summary>
        public string Name
        {
            get
            {
                return this.Info.Name;
            }
        }

        /// <summary>
        /// 获取参数的值
        /// 该值为线程独立保存
        /// </summary>
        public object Value
        {
            get
            {
                return this.localValue.IsValueCreated ? this.localValue.Value : DBNull.Value;
            }
            set
            {
                this.localValue.Value = value;
            }
        }

        /// <summary>
        /// Api参数
        /// </summary>
        /// <param name="info">参数信息</param>
        /// <param name="index">参数索引</param>     
        internal ApiParameter(ParameterInfo info, int index)
        {
            this.Info = info;
            this.Index = index;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
