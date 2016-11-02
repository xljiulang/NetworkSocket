using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Fast
{
    public class FastLoginAttribute : FastFilterAttribute
    {
        protected override void OnExecuting(ActionContext filterContext)
        {
            if (filterContext.Session.Tag.Get("logined").IsNull == true)
            {
                filterContext.Result = new NetworkSocket.Exceptions.RemoteException("请先登录再操作");
            }
        }
    }
}
