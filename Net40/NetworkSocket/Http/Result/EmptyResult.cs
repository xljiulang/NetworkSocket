using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http空内容的结果
    /// </summary>
    public class EmptyResult : ContentResult
    {
        /// <summary>
        /// 空内容的结果
        /// </summary>
        public EmptyResult()
            : base(null)
        {
        }
    }
}
