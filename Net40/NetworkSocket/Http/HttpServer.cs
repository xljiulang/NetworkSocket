using NetworkSocket.Core;
using NetworkSocket.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http服务
    /// </summary>
    public class HttpServer : HttpServerBase
    {
        /// <summary>
        /// 所有Http行为
        /// </summary>
        private HttpActionList httpActionList;

        /// <summary>
        /// 获取模型生成器
        /// </summary>
        public IModelBinder ModelBinder { get; private set; }

        /// <summary>
        /// 获取全局过滤器
        /// </summary>
        public GlobalFilters GlobalFilter { get; private set; }

        /// <summary>
        /// 特性过滤器提供者
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; private set; }

        /// <summary>
        /// 获取或设置依赖关系解析提供者
        /// </summary>
        public IDependencyResolver DependencyResolver { get; set; }

        /// <summary>
        /// Http服务
        /// </summary>
        public HttpServer()
        {
            this.httpActionList = new HttpActionList();
            this.ModelBinder = new DefaultModelBinder();
            this.GlobalFilter = new GlobalFilters();
            this.FilterAttributeProvider = new FilterAttributeProvider();
            this.DependencyResolver = new DefaultDependencyResolver();
        }

        /// <summary>
        /// 注册所有控制器         
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void RegisterControllers(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException();
            }
            var controllers = assembly.GetTypes().Where(item => typeof(HttpController).IsAssignableFrom(item) && item.IsAbstract == false);
            foreach (var controller in controllers)
            {
                var httpActions = GetControllerHttpActions(controller);
                this.httpActionList.AddRange(httpActions);
            }
        }

        /// <summary>
        /// 获取服务类型的Api行为
        /// </summary>
        /// <param name="controller">服务类型</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        private static IEnumerable<HttpAction> GetControllerHttpActions(Type controller)
        {
            return controller
                .GetMethods()
                .Where(item => typeof(ActionResult).IsAssignableFrom(item.ReturnType))
                .Select(method => new HttpAction(method));
        }

        /// <summary>
        /// 收到Http请求
        /// </summary>       
        /// <param name="request">请求对象</param>
        /// <param name="response">回复对象</param>
        protected override void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            var route = request.Url.AbsolutePath;
            var action = this.httpActionList.TryGet(route);
            var requestContext = new RequestContext(request, response);

            if (action == null)
            {
                var exception = new ApiNotExistException(route);
                var exceptionContext = new ExceptionContext(requestContext, exception);
                foreach (var filter in this.GlobalFilter.ExceptionFilters)
                {
                    filter.OnException(exceptionContext);
                    if (exceptionContext.ExceptionHandled == true) break;
                }

                var result = exceptionContext.Result == null ? new ErrorResult { Status = 404, Errors = exception.Message } : exceptionContext.Result;
                result.ExecuteResult(requestContext);
            }
            else
            {
                var controller = this.DependencyResolver.GetService(action.DeclaringService) as HttpController;
                var actionContext = new ActionContext(requestContext, action);

                controller.Server = this;
                ((IHttpController)controller).Execute(actionContext);
            }
        }
    }
}
