using HttpServer.Filters;
using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HttpServer.Controller
{
    /// <summary>
    /// 电源控制器
    /// </summary>
    public class PowerController : HttpController
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
        /// 重启
        /// </summary>
        /// <returns></returns>
        [LogFilter("重启")]
        public JsonResult Reboot()
        {
            var state = PowerHelper.ExitWindows(ExitCode.Reboot | ExitCode.ForceIfHung);
            return Json(state);
        }

        /// <summary>
        /// 关机
        /// </summary>
        /// <returns></returns>
        [LogFilter("关机")]
        public JsonResult Shutdown()
        {
            var state = PowerHelper.ExitWindows(ExitCode.ShutDown | ExitCode.ForceIfHung);
            return Json(state);
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        [LogFilter("注销")]
        public JsonResult Logoff()
        {
            var state = PowerHelper.ExitWindows(ExitCode.LogOff | ExitCode.ForceIfHung);
            return Json(state);
        }
    }
}
