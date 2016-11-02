using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace Service.Http
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
            var file = string.Format("View\\{0}\\{1}.cshtml", aciton.ControllerName, aciton.Method.Name);
            var html = System.IO.File.ReadAllText(file, Encoding.Default);
            return Content(html);
        }

        /// <summary>
        /// 服务器事件订阅
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 用户提交了文章
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string article)
        {
            var httpEvent = new HttpEvent { Data = article };
            this.CurrentContext
                .EventSession
                .ToList()
                .ForEach(item => item.SendEvent(httpEvent));

            return Json("你的文章已推荐给所有人");
        }

        /// <summary>
        /// SSE请求
        /// </summary>
        /// <returns></returns>
        public ActionResult Source()
        {
            if (Request.IsEventStreamRequest() == true)
            {
                return new EventResult();
            }
            return new EmptyResult();
        }
    }
}
