using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using NetworkSocket.Tasks;
using NetworkSocket.Core;

namespace Service.Http
{
    /// <summary>
    /// WebApi控制器
    /// </summary>
    public class WebApiController : HttpController
    {
        /// <summary>
        /// /WebApi/About
        /// </summary>
        /// <returns></returns>     
        public object About()
        {
            var names = typeof(HttpController).Assembly.GetName();
            return new { assembly = names.Name, version = names.Version.ToString() };
        }

        /// <summary>
        /// /V2/WebApi/About
        /// </summary>
        /// <returns></returns>
        [Route("/v2/{controller}/about")]
        public object About_V2()
        {
            var names = typeof(HttpController).Assembly.GetName();
            return new { assembly = names.Name, version = names.Version.ToString() };
        }


        /// <summary>
        /// 伪静态 /WebApi/Login.html
        /// async await 异步
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/{controller}/{action}.html")]
        public async Task<object> Login([NotNull]string account, [NotNull] string password)
        {
            await Task.FromResult(0);
            return new { account, password };
        }

        /// <summary>
        /// /NetworkSocket/RouteDataTest
        /// </summary>
        /// <returns></returns>
        [Route("/{namespace}/{action}")]
        public string GetNamespace()
        {
            var space = this.CurrentContext.Action.RouteData["namespace"];
            return space;
        }

        /// <summary>
        /// /NetworkSocket/WebApi/RouteDataTest
        /// </summary>
        /// <returns></returns>
        [Route("/{namespace}/{controller}/GetNamespace")]
        public string GetNamespaceWithController()
        {
            var space = this.CurrentContext.Action.RouteData["namespace"];
            return space;
        }
    }
}
