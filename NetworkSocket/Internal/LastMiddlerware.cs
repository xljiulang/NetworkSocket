using NetworkSocket.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 表示最后一个中间件
    /// </summary>
    internal class LastMiddlerware : IMiddleware
    {
        /// <summary>
        /// 下一个中间件
        /// </summary>
        public IMiddleware Next { set; private get; }

        /// <summary>
        /// 执行中间件          
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public Task Invoke(IContenxt context)
        {
            context.StreamReader.Clear();
            context.Session.Close();
            return TaskExtend.CompletedTask;
        }
    }
}
