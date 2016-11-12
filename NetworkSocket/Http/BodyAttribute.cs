using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示标记参数为application/json的请求类型对应的body内容
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class BodyAttribute : Attribute
    {
    }
}
