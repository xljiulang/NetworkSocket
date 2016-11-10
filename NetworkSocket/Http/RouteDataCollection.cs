using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 路由数据集合
    /// </summary>
    public class RouteDataCollection : NameValueCollection
    {
        /// <summary>
        /// 路由数据集合
        /// </summary>
        public RouteDataCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// 清除
        /// </summary>
        public override void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 移除项
        /// </summary>
        /// <param name="name">项名</param>
        public override void Remove(string name)
        {
            throw new NotSupportedException();
        }
    }
}
