using NetworkSocket.Http;
using MixServer.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace MixServer.Controller
{
    /// <summary>
    /// 电源控制器
    /// </summary>
    public class HomeController : HttpController
    {
        /// <summary>
        /// 加载视图
        /// </summary>
        /// <returns></returns>
        private ActionResult View()
        {
            var aciton = this.CurrentContext.Action;
            var file = string.Format("View\\{0}\\{1}.cshtml", aciton.ControllerName, aciton.ActionName);
            var html = System.IO.File.ReadAllText(file, Encoding.Default);
            return Content(html);
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// WebApi示例页面
        /// </summary>
        /// <returns></returns>
        public ActionResult WebApi()
        {
            return View();
        }
         

        /// <summary>
        /// 中间件编写代码示例页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Middleware()
        {
            return View();
        }
    }
}
