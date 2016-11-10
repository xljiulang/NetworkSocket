using NetworkSocket.Reflection;
using NetworkSocket.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        /// 类过滤器
        /// </summary>
        private readonly FilterAttribute[] classFiltersCache;

        /// <summary>
        /// 方法过滤器
        /// </summary>
        private readonly FilterAttribute[] methodFiltersCache;

        /// <summary>
        /// 参数过滤器
        /// </summary>
        private readonly ParameterFilterAttribute[] parametersFiltersCache;



        /// <summary>
        /// 是否是Task类型返回
        /// </summary>
        private readonly bool isTaskReturn;

        /// <summary>
        /// 获取方法成员信息
        /// </summary>
        public Method Method { get; private set; }

        /// <summary>
        /// 获取Api行为的Api名称
        /// </summary>
        public string ApiName { get; private set; }

        /// <summary>
        /// 获取Api行为的方法成员返回类型是否为void
        /// </summary>
        public bool IsVoidReturn { get; private set; }

        /// <summary>
        /// Api行为的方法成员返回类型
        /// </summary>
        public Type ReturnType { get; private set; }

        /// <summary>
        /// 获取声明该成员的服务类型
        /// </summary>
        public Type DeclaringService { get; protected set; }

        /// <summary>
        /// 获取Api参数
        /// </summary>
        public ApiParameter[] Parameters { get; private set; }

        /// <summary>
        /// Api行为
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <exception cref="ArgumentException"></exception>
        public ApiAction(MethodInfo method)
        {
            this.Method = new Method(method);
            this.ApiName = this.GetApiName(method);
            this.ReturnType = method.ReturnType;
            this.DeclaringService = method.DeclaringType;
            this.isTaskReturn = typeof(Task).IsAssignableFrom(method.ReturnType);
            this.IsVoidReturn = method.ReturnType.Equals(typeof(void)) || method.ReturnType.Equals(typeof(Task));
            this.Parameters = method.GetParameters().Select((p, i) => new ApiParameter(p, i)).ToArray();

            this.classFiltersCache = this.GetClassFilterAttributes(cache: false).ToArray();
            this.methodFiltersCache = this.GetMethodFilterAttributes(cache: false).ToArray();
            this.parametersFiltersCache = this.GetParametersFilterAttributes(cache: false).ToArray();
        }

        /// <summary>
        /// 获取ApiName
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns></returns>
        private string GetApiName(MethodInfo method)
        {
            var api = Attribute.GetCustomAttribute(method, typeof(ApiAttribute)) as ApiAttribute;
            if (api != null && string.IsNullOrWhiteSpace(api.Name) == false)
            {
                return api.Name;
            }
            else
            {
                return Regex.Replace(method.Name, @"Async$", string.Empty, RegexOptions.IgnoreCase);
            }
        }

        /// <summary>
        /// 获取类级过滤器特性
        /// </summary>
        /// <param name="cache">是否从缓存读取</param>
        /// <returns></returns>
        public virtual IEnumerable<FilterAttribute> GetClassFilterAttributes(bool cache)
        {
            if (cache == false)
            {
                return this.DeclaringService.GetCustomAttributes<FilterAttribute>(inherit: true);
            }
            else
            {
                return this.classFiltersCache;
            }
        }

        /// <summary>
        /// 获取方法级过滤器特性
        /// </summary>
        /// <param name="cache">是否从缓存读取</param>
        /// <returns></returns>
        public virtual IEnumerable<FilterAttribute> GetMethodFilterAttributes(bool cache)
        {
            if (cache == false)
            {
                return this.Method.Info.GetCustomAttributes<FilterAttribute>(inherit: true);
            }
            else
            {
                return this.methodFiltersCache;
            }
        }

        /// <summary>
        /// 获取参数的参数过滤器
        /// </summary>
        /// <param name="cache">是否从缓存读取</param>
        /// <returns></returns>
        public virtual IEnumerable<ParameterFilterAttribute> GetParametersFilterAttributes(bool cache)
        {
            if (cache == false)
            {
                return this.Parameters.SelectMany(p =>
                    p.Info
                    .GetCustomAttributes<ParameterFilterAttribute>(inherit: true)
                    .Select(f => f.BindParameter(p)));
            }
            else
            {
                return this.parametersFiltersCache;
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
            if (this.isTaskReturn == true)
            {
                var task = this.Execute(service, parameters) as Task;
                return task == null ? Task.FromResult<object>(null) : task.ToTask<object>(this.ReturnType);
            }
            else
            {
                return Task.Run<object>(() => this.Execute(service, parameters));
            }
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
