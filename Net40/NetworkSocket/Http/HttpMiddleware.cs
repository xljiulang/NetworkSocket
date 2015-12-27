using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Http
{
    /// <summary>
    /// 表示Http中间件
    /// </summary>
    public class HttpMiddleware : HttpMiddlewareBase, IDependencyResolverSupportable, IFilterSupportable
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
        /// 获取全局过滤器管理者
        /// </summary>
        public IGlobalFilters GlobalFilters { get; private set; }

        /// <summary>
        /// 获取MIME集合
        /// </summary>
        public HttpMIMECollection MIMECollection { get; private set; }

        /// <summary>
        /// 获取或设置特性过滤器提供者
        /// 默认提供者解析为单例模式
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 获取或设置依赖关系解析提供者
        /// </summary>
        public IDependencyResolver DependencyResolver { get; set; }

        /// <summary>
        /// Http服务
        /// </summary>
        public HttpMiddleware()
        {
            this.httpActionList = new HttpActionList();
            this.ModelBinder = new DefaultModelBinder();
            this.GlobalFilters = new HttpGlobalFilters();
            this.MIMECollection = new HttpMIMECollection();
            this.FilterAttributeProvider = new DefaultFilterAttributeProvider();
            this.DependencyResolver = new DefaultDependencyResolver();

            this.MIMECollection.FillBasicMIME();
            DomainAssembly.GetAssemblies().ForEach(item => this.BindController(item));
        }

        /// <summary>
        /// 绑定程序集下的所有控制器
        /// </summary>
        /// <param name="assembly">程序集</param>
        private void BindController(Assembly assembly)
        {
            var controllers = assembly
                .GetTypes()
                .Where(item => item.IsAbstract == false)
                .Where(item => typeof(HttpController).IsAssignableFrom(item));

            foreach (var controller in controllers)
            {
                var httpActions = HttpMiddleware.GetControllerHttpActions(controller);
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
                .Select(method => new HttpAction(method, controller));
        }

        /// <summary>
        /// 收到Http请求时触发
        /// </summary>       
        /// <param name="context">上下文</param>
        /// <param name="requestContext">请求上下文对象</param>
        protected override void OnHttpRequest(IContenxt context, RequestContext requestContext)
        {
            var extenstion = Path.GetExtension(requestContext.Request.Path);
            if (string.IsNullOrWhiteSpace(extenstion) == false)
            {
                this.ProcessStaticFileRequest(extenstion, requestContext);
            }
            else
            {
                this.ProcessActionRequest(requestContext.Request.Path, context, requestContext);
            }
        }

        /// <summary>
        /// 处理静态资源请求
        /// </summary>
        /// <param name="extension">扩展名</param>
        /// <param name="requestContext">上下文</param>
        private void ProcessStaticFileRequest(string extension, RequestContext requestContext)
        {
            var contenType = this.MIMECollection[extension];
            var file = requestContext.Request.Url.AbsolutePath.TrimStart('/').Replace(@"/", @"\");

            if (string.IsNullOrWhiteSpace(contenType) == true)
            {
                var ex = new HttpException(403, string.Format("未配置{0}格式的MIME ..", extension));
                this.ProcessHttpException(ex, requestContext);
            }
            else if (File.Exists(file) == false)
            {
                var ex = new HttpException(404, string.Format("找不到文件{0} ..", file));
                this.ProcessHttpException(ex, requestContext);
            }
            else
            {
                var result = new FileResult { FileName = file, ContentType = contenType };
                result.ExecuteResult(requestContext);
            }
        }


        /// <summary>
        /// 处理一般的请求
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="context">上下文</param>
        /// <param name="requestContext">请求上下文</param>
        private void ProcessActionRequest(string route, IContenxt context, RequestContext requestContext)
        {
            var action = this.httpActionList.TryGet(requestContext.Request);
            if (action == null)
            {
                var ex = new HttpException(404, "找不到路径" + route);
                this.ProcessHttpException(ex, requestContext);
            }
            else
            {
                this.ExecuteHttpAction(action, context, requestContext);
            }
        }

        /// <summary>
        /// 执行httpAction
        /// </summary>
        /// <param name="action">httpAction</param>
        /// <param name="context">上下文</param>
        /// <param name="requestContext">请求上下文</param>      
        private void ExecuteHttpAction(HttpAction action, IContenxt context, RequestContext requestContext)
        {
            var actionContext = new ActionContext(requestContext, action, context);
            var controller = GetHttpController(actionContext);

            if (controller != null)
            {
                controller.Execute(actionContext);
                this.DependencyResolver.TerminateService(controller);
            }
        }

        /// <summary>
        /// 获取控制器的实例
        /// </summary>
        /// <param name="actionContext">上下文</param>
        /// <returns></returns>
        private IHttpController GetHttpController(ActionContext actionContext)
        {
            try
            {
                var controllerType = actionContext.Action.DeclaringService;
                var controller = this.DependencyResolver.GetService(controllerType) as HttpController;
                controller.Server = this;
                return controller;
            }
            catch (Exception ex)
            {
                var httpException = new HttpException(500, ex.Message);
                this.ProcessHttpException(httpException, actionContext);
                return null;
            }
        }

        /// <summary>
        /// 异常时
        /// </summary>
        /// <param name="session">产生异常的会话</param>
        /// <param name="exception">异常</param>
        protected sealed override void OnException(ISession session, Exception exception)
        {
            var response = new HttpResponse(session);
            var requestContext = new RequestContext(null, response);
            var exceptionConext = new ExceptionContext(requestContext, exception);
            this.ExecGlobalExceptionFilters(exceptionConext);

            if (exceptionConext.Result != null)
            {
                exceptionConext.Result.ExecuteResult(requestContext);
            }
            else
            {
                base.OnException(session, exception);
            }
        }

        /// <summary>
        /// 处理Http异常
        /// </summary>
        /// <param name="ex">Http异常</param>
        /// <param name="context">请求上下文</param>
        private void ProcessHttpException(HttpException ex, RequestContext context)
        {
            var exceptionContent = new ExceptionContext(context, ex);
            this.ExecGlobalExceptionFilters(exceptionContent);
            var result = exceptionContent.Result ?? new ErrorResult(ex);
            result.ExecuteResult(context);
        }

        /// <summary>
        /// 执行全局异常过滤器
        /// </summary>         
        /// <param name="exceptionContext">上下文</param>       
        private void ExecGlobalExceptionFilters(ExceptionContext exceptionContext)
        {
            if (this.GlobalFilters.Count == 0)
            {
                return;
            }

            foreach (IFilter filter in this.GlobalFilters)
            {
                filter.OnException(exceptionContext);
                if (exceptionContext.ExceptionHandled == true) break;
            }

            if (exceptionContext.ExceptionHandled == false)
            {
                throw exceptionContext.Exception;
            }
        }
    }
}
