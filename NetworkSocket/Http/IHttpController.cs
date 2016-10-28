using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 定义Http控制器的接口
    /// </summary>
    public interface IHttpController : IDisposable
    {
        /// <summary>
        /// 异步执行Http行为
        /// </summary>              
        /// <param name="actionContext">Api行为上下文</param>   
        /// <returns></returns>
        Task ExecuteAsync(ActionContext actionContext);
    }
}
