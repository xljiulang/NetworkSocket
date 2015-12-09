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
    /// 表示Http服务
    /// </summary>
    public class HttpServer : HttpServerBase, IDependencyResolverSupportable, IFilterSupportable
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
        /// </summary>
        public IFilterAttributeProvider FilterAttributeProvider { get; set; }

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
            this.GlobalFilters = new GlobalFilters();
            this.MIMECollection = new HttpMIMECollection();
            this.FilterAttributeProvider = new DefaultFilterAttributeProvider();
            this.DependencyResolver = new DefaultDependencyResolver();

            this.MIMECollection.FillBasicMIME();
        }

        /// <summary>
        /// 绑定程序集下的所有控制器         
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public HttpServer BindController(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException();
            }
            var controllers = assembly.GetTypes().Where(item => typeof(HttpController).IsAssignableFrom(item) && item.IsAbstract == false);
            return this.BindController(controllers);
        }


        /// <summary>
        /// 绑定控制器
        /// </summary>
        /// <typeparam name="TController">控制器类型</typeparam>        
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public HttpServer BindController<TController>()
        {
            return this.BindController(typeof(TController));
        }

        /// <summary>
        /// 绑定控制器
        /// </summary>
        /// <param name="controllers">控制器</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public HttpServer BindController(params Type[] controllers)
        {
            if (controllers == null)
            {
                throw new ArgumentNullException();
            }
            return this.BindController((IEnumerable<Type>)controllers);
        }

        /// <summary>
        /// 绑定控制器
        /// </summary>
        /// <param name="controllers">控制器</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public HttpServer BindController(IEnumerable<Type> controllers)
        {
            if (controllers == null)
            {
                throw new ArgumentNullException();
            }

            if (controllers.Any(item => typeof(HttpController).IsAssignableFrom(item) == false || item.IsAbstract))
            {
                throw new ArgumentException("类型必须派生于HttpController且不为抽象类");
            }

            foreach (var controller in controllers)
            {
                var httpActions = HttpServer.GetControllerHttpActions(controller);
                this.httpActionList.AddRange(httpActions);
            }
            return this;
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
        /// 收到Http请求
        /// </summary>       
        /// <param name="request">请求对象</param>
        /// <param name="response">回复对象</param>
        protected override void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            Task.Factory.StartNew(() => this.ProcessRequest(new RequestContext(request, response)));
        }

        /// <summary>
        /// 处理http请求
        /// </summary>
        /// <param name="requestContext">请求上下文</param>
        private void ProcessRequest(RequestContext requestContext)
        {
            var extenstion = Path.GetExtension(requestContext.Request.Path);

            if (string.IsNullOrWhiteSpace(extenstion) == false)
            {
                this.ProcessStaticFileRequest(extenstion, requestContext);
            }
            else
            {
                this.ProcessNormalRequest(requestContext.Request.Path, requestContext);
            }
        }

        /// <summary>
        /// 处理静态资源请求
        /// </summary>
        /// <param name="extension">扩展名</param>
        /// <param name="requestContext">上下文</param>
        private void ProcessStaticFileRequest(string extension, RequestContext requestContext)
        {
            var contenType = this.MIMECollection.GetContentType(extension);
            var file = requestContext.Request.Url.AbsolutePath.TrimStart('/').Replace(@"/", @"\");

            if (string.IsNullOrWhiteSpace(contenType) == true)
            {
                var result = new ErrorResult { Status = 403 };
                result.ExecuteResult(requestContext);
            }
            else if (File.Exists(file) == false)
            {
                var result = new ErrorResult { Status = 404, Errors = "找不到指定的文件 .." };
                result.ExecuteResult(requestContext);
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
        /// <param name="requestContext">上下文</param>
        private void ProcessNormalRequest(string route, RequestContext requestContext)
        {
            var action = this.httpActionList.TryGet(requestContext.Request);
            if (action == null)
            {
                this.ProcessActionNotFound(route, requestContext);
            }
            else
            {
                this.ExecuteHttpAction(action, requestContext);
            }
        }

        /// <summary>
        /// 处理找不到Action
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="requestContext">上下文</param>
        private void ProcessActionNotFound(string route, RequestContext requestContext)
        {
            var exception = new ApiNotExistException(route);
            var exceptionContext = new ExceptionContext(requestContext, exception);
            foreach (IFilter filter in this.GlobalFilters)
            {
                filter.OnException(exceptionContext);
                if (exceptionContext.ExceptionHandled == true) break;
            }

            var result = exceptionContext.Result != null ? exceptionContext.Result : new ErrorResult
            {
                Status = 404,
                Errors = exception.Message
            };
            result.ExecuteResult(requestContext);
        }

        /// <summary>
        /// 执行httpAction
        /// </summary>
        /// <param name="action">httpAction</param>
        /// <param name="requestContext">上下文</param>      
        private void ExecuteHttpAction(HttpAction action, RequestContext requestContext)
        {
            var controller = this.DependencyResolver.GetService(action.DeclaringService) as HttpController;
            var actionContext = new ActionContext(requestContext, action);

            controller.Server = this;
            ((IHttpController)controller).Execute(actionContext);

            // 释放资源
            this.DependencyResolver.TerminateService(controller);
        }
    }
}
