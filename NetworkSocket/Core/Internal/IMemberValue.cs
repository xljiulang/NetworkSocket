using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 定义成员操作的接口
    /// </summary>
    internal interface IMemberValue
    {
        /// <summary>
        /// 获取成员的值 
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        bool TryGetValue(string memberName, out object value);
    }
}
