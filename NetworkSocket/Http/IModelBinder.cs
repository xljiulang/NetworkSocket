using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 定义绑定参数模型的接口
    /// </summary>
    public interface IModelBinder
    {
        /// <summary>
        /// 生成和绑定所有参数的值
        /// </summary>
        /// <param name="context">上下文</param>
        void BindAllParameterValue(ActionContext context);
    }
}
