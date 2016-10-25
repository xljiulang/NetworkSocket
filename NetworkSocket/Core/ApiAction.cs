using NetworkSocket.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Core
{
    /// <summary>
    /// 表示Api行为    
    /// </summary>
    [DebuggerDisplay("ApiName = {ApiName}")]
    public class ApiAction
    {
        /// <summary>
        /// 参数值
        /// </summary>
        [ThreadStatic]
        private static object[] parameters;

        /// <summary>
        /// 获取Api行为的Api名称
        /// </summary>
        public string ApiName { get; protected set; }

        /// <summary>
        /// 获取Api行为的方法成员返回类型是否为void
        /// </summary>
        public bool IsVoidReturn { get; protected set; }

        /// <summary>
        /// Api行为的方法成员返回类型
        /// </summary>
        public Type ReturnType { get; protected set; }

        /// <summary>
        /// 获取Api行为的参数信息
        /// </summary>
        public ParameterInfo[] ParameterInfos { get; protected set; }

        /// <summary>
        /// 获取Api行为的方法成员参数类型
        /// </summary>
        public Type[] ParameterTypes { get; protected set; }

        /// <summary>
        /// 获取Api行为的参数值
        /// </summary>
        public object[] ParameterValues
        {
            get
            {
                return parameters;
            }
            internal set
            {
                parameters = value;
            }
        }


        /// <summary>
        /// 获取方法成员信息
        /// </summary>
        public Method Method { get; private set; }

        /// <summary>
        /// 获取声明该成员的服务类型
        /// </summary>
        public Type DeclaringService { get; protected set; }


        /// <summary>
        /// Api行为
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <exception cref="ArgumentException"></exception>
        public ApiAction(MethodInfo method)
        {
            this.Method = new Method(method);
            this.DeclaringService = method.DeclaringType;
            this.ReturnType = method.ReturnType;
            this.IsVoidReturn = method.ReturnType.Equals(typeof(void)) || method.ReturnType.Equals(typeof(Task));
            this.ParameterInfos = method.GetParameters();
            this.ParameterTypes = this.ParameterInfos.Select(item => item.ParameterType).ToArray();

            var api = Attribute.GetCustomAttribute(method, typeof(ApiAttribute)) as ApiAttribute;
            if (api != null && string.IsNullOrWhiteSpace(api.Name) == false)
            {
                this.ApiName = api.Name;
            }
            else
            {
                this.ApiName = method.Name;
            }
        }


        /// <summary>
        /// 获取Api行为或Api行为的声明类型是否声明了特性
        /// </summary>
        /// <param name="type">特性类型</param>
        /// <param name="inherit">是否继承</param>
        /// <returns></returns>
        public bool IsDefined(Type type, bool inherit)
        {
            return this.Method.Info.IsDefined(type, inherit) || this.DeclaringService.IsDefined(type, inherit);
        }

        /// <summary>
        /// 获取方法级过滤器特性
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<FilterAttribute> GetMethodFilterAttributes()
        {
            return Attribute.GetCustomAttributes(this.Method.Info, typeof(FilterAttribute), true).Cast<FilterAttribute>();
        }

        /// <summary>
        /// 获取类级过滤器特性
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<FilterAttribute> GetClassFilterAttributes()
        {
            return Attribute.GetCustomAttributes(this.DeclaringService, typeof(FilterAttribute), true).Cast<FilterAttribute>();
        }

        /// <summary>
        /// 执行Api行为
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="parameters">参数实例</param>
        /// <returns></returns>
        public object Execute(object service, params object[] parameters)
        {
            return this.Method.Invoke(service, parameters);
        }

        /// <summary>
        /// 异步执行Api行为
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="parameters">参数实例</param>
        /// <returns></returns>
        public Task<object> ExecuteAsync(object service, params object[] parameters)
        {
            var result = this.Execute(service, parameters);
            return TaskEx.CastFrom(result, this.ReturnType);
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ApiName;
        }
    }
}
