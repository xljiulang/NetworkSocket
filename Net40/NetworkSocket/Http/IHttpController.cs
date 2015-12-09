using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 定义Http控制器的接口
    /// </summary>
    public interface IHttpController : IDisposable
    {
        /// <summary>
        /// 执行Api行为
        /// </summary>              
        /// <param name="actionContext">Api行为上下文</param>      
        void Execute(ActionContext actionContext);
    }
}
