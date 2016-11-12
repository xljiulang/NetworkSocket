using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示路由数据
    /// </summary>
    public class RouteData
    {
        /// <summary>
        /// 获取键
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 获取值
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// 路由数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public RouteData(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
