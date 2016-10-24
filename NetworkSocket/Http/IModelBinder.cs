using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 定义模型生成器的接口
    /// </summary>
    public interface IModelBinder
    {
        /// <summary>
        /// 生成参数的模型
        /// </summary>
        /// <param name="request">http请求</param>
        /// <param name="parameter">HttpAction的一个参数</param>       
        /// <returns></returns>
        object BindModel(HttpRequest request, ParameterInfo parameter);
    }
}
