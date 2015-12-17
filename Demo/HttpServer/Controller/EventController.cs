using HttpServer.Filters;
using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServer.Controller
{
    /// <summary>
    /// Http事件推送控制器
    /// </summary>
    public class EventController : HttpController
    {
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
