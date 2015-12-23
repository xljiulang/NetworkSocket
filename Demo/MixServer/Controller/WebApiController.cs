using NetworkSocket.Http;
using MixServer.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using NetworkSocket.Fast;

namespace MixServer.Controller
{
    /// <summary>
    /// WebApi控制器
    /// </summary>
    public class WebApiController : HttpController
    {
        public JsonResult About()
        {
            var names = typeof(HttpController).Assembly.GetName();
            return Json(new { assembly = names.Name, version = names.Version.ToString() });
        }

        [HttpPost]
        public JsonResult Login(string account, string password)
        {
            return Json(new { account, password });
        }
    }
}
