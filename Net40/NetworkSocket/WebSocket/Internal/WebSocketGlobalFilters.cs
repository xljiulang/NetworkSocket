using NetworkSocket.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// JsonWebSocket协议的全局过滤器提供者
    /// </summary>
    internal class WebSocketGlobalFilters : GlobalFiltersBase
    {
        /// <summary>
        /// 添加过滤器
        /// </summary>
        /// <param name="filter"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public override void Add(IFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException();
            }

            var fastFilter = filter as JsonWebSocketFilterAttribute;
            if (fastFilter == null)
            {
                throw new ArgumentException("过滤器的类型要继承于" + typeof(JsonWebSocketFilterAttribute).Name);
            }
            base.Add(filter);
        }
    }
}
