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
        /// 启动fastClient
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RunFastApp()
        {
#if DEBUG
            var file = @"..\FastClient\bin\Debug\FastClient.exe";
#else
             var file = @"..\FastClient\bin\Release\FastClient.exe";
#endif
            if (System.IO.File.Exists(file))
            {
                Process.Start(file);
                return Json(new { state = false });
            }
            return Json(new { state = false, message = "请先编译好FastClient项目 .." });
        }

        /// <summary>
        /// 消息推送到fast客户端
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Message(string message)
        {
            this.CurrentContext
                .AllSessions
                .FilterWrappers<FastSession>()
                .ToList()
                .ForEach(item => item.InvokeApi("HttpNotify", message));

            return new EmptyResult();
        }       
    }
}
