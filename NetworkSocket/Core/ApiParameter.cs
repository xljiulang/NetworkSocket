using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示Api参数
    /// </summary>
    public sealed class ApiParameter
    {
        /// <summary>
        /// 关联的api
        /// </summary>
        private readonly ApiAction instance;

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
        /// </summary>
        public object Value
        {
            get
            {
                if (this.instance.ParametersValues != null && this.instance.ParametersValues.Length > this.Index)
                {
                    return this.instance.ParametersValues[this.Index];
                }
                else
                {
                    return DBNull.Value;
                }
            }
        }

        /// <summary>
        /// Api参数
        /// </summary>
        /// <param name="instance">关联的api</param>
        /// <param name="info">参数信息</param>
        /// <param name="index">参数索引</param>     
        internal ApiParameter(ApiAction instance, ParameterInfo info, int index)
        {
            this.instance = instance;
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
