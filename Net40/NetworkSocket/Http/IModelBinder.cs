using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 模型生成器
    /// </summary>
    public interface IModelBinder
    {
        /// <summary>
        /// 生成模型
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="parameter">参数</param>       
        /// <returns></returns>
        object BindModel(HttpRequest request, ParameterInfo parameter);
    }
}
