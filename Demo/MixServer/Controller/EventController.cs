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
    /// SSE控制器
    /// </summary>
    public class EventController : HttpController
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
        /// 服务器事件订阅
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (Request.IsEventStreamRequest() == true)
            {
                return new EventResult();
            }
            return Content("不是有效的SSE请求 ..");
        }
    }
}
