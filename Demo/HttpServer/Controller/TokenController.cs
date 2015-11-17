using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.Http;
using HttpServer.Filters;

namespace HttpServer.Controller
{
    [Route("/api/token")]
    public class TokenController : HttpController
    {
        public class model
        {
            public int x { get; set; }
            public int y { get; set; }
        }

        [LogFilter("Test请求了")]
        public ActionResult Test(model m, DateTime? t)
        {
            return Json(new { m.x, m.y });
        }
    }
}
