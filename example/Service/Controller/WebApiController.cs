using NetworkSocket.Http;
using Service.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using NetworkSocket.Tasks;

namespace Service.Controller
{
    /// <summary>
    /// WebApi控制器
    /// </summary>
    public class WebApiController : HttpController
    {
        [Route("/{version}/{controller}/{action}")]
        public object About()
        {
            var names = typeof(HttpController).Assembly.GetName();
            return new { assembly = names.Name, version = names.Version.ToString() };
        }

        [HttpPost]
        public async Task<object> Login(string account, string password)
        {
            await Task.Delay(3 * 1000);
            return new { account, password };
        }
    }
}
