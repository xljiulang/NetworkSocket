using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpServer.Controller
{
    public class HomeController : HttpController
    {
        public class User
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        public ActionResult Index(User user, int?[] x, string[] y, int z = 4)
        {
            return Content("OK");
        }
    }
}
