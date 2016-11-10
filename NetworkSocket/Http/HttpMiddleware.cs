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
        /// http行为表
        /// </summary>
        private readonly HttpActionTable httpActionTable;

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
            var httpActions = this.FindAllHttpActions();
            this.httpActionTable = new HttpActionTable(httpActions);

            this.ModelBinder = new DefaultModelBinder();
            this.GlobalFilters = new HttpGlobalFilters();
            this.MIMECollection = new HttpMIMECollection();
            this.FilterAttributeProvider = new DefaultFilterAttributeProvider();
            this.DependencyResolver = new DefaultDependencyResolver();

            this.MIMECollection.FillBasicMIME();
        }

        /// <summary>
        /// 搜索程序域内所有HttpAction
        /// </summary>
        /// <returns></returns>
        private IEnumerable<HttpAction> FindAllHttpActions()
        {
            return DomainAssembly
                .GetAssemblies()
                .SelectMany(item => this.GetHttpActions(item));
        }

        /// <summary>
        /// 获取程序集的Api行为
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <returns></returns>
        private IEnumerable<HttpAction> GetHttpActions(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(item => item.IsAbstract == false)
                .Where(item => typeof(HttpController).IsAssignableFrom(item))
                .SelectMany(item => this.GetHttpActions(item));
        }

        /// <summary>
        /// 获取服务类型的Api行为
        /// </summary>
        /// <param name="controller">服务类型</param>
        /// <returns></returns>
        private IEnumerable<HttpAction> GetHttpActions(Type controller)
        {
            return controller
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Where(item => HttpAction.IsHttpAction(item))
                .Select(method => new HttpAction(method, controller));
        }

        /// <summary>
        /// 收到Http请求时触发
        /// </summary>       
        /// <param name="context">上下文</param>
        /// <param name="requestContext">请求上下文对象</param>
        /// <returns></returns>
        protected override async Task OnHttpRequestAsync(IContenxt context, RequestContext requestContext)
        {
            var action = this.httpActionTable.TryGetAndClone(requestContext.Request);
            if (action != null)
            {
                await this.ExecuteHttpActionAsync(action, context, requestContext);
            }
            else
            {
                await this.ProcessStaticFileRequestAsync(context, requestContext);
            }
        }

        /// <summary>
        /// 处理静态资源请求
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="requestContext">请求上下文对象</param>
        /// <returns></returns>
        private async Task ProcessStaticFileRequestAsync(IContenxt context, RequestContext requestContext)
        {
            const string currentDirectory = ".";
            var localPath = currentDirectory + requestContext.Request.Url.LocalPath;
            var extension = Path.GetExtension(localPath);

            if (string.IsNullOrWhiteSpace(extension) == true)
            {
                var exception = new HttpException(404, "请求的页面不存在");
                this.OnException(context.Session, exception);
            }
            else if (this.MIMECollection.Contains(extension) == false)
            {
                var exception = new HttpException(403, string.Format("未配置{0}格式的MIME ..", extension));
                this.OnException(context.Session, exception);
            }
            else
            {
                var contenType = this.MIMECollection[extension];
                var fileResult = new FileResult { FileName = localPath, ContentType = contenType };
                await fileResult.ExecuteResultAsync(requestContext);
            }
        }

        /// <summary>
        /// 执行httpAction
        /// </summary>
        /// <param name="action">httpAction</param>
        /// <param name="context">上下文</param>
        /// <param name="requestContext">请求上下文</param>  
        /// <returns></returns>
        private async Task ExecuteHttpActionAsync(HttpAction action, IContenxt context, RequestContext requestContext)
        {
            try
            {
                var actionContext = new ActionContext(requestContext, action, context);
                var controller = this.GetHttpController(actionContext);
                await controller.ExecuteAsync(actionContext);
                this.DependencyResolver.TerminateService(controller);
            }
            catch (Exception ex)
            {
                this.OnException(context.Session, ex);
            }
        }

        /// <summary>
        /// 获取控制器的实例
        /// </summary>
        /// <param name="actionContext">上下文</param>
        /// <exception cref="ResolveException"></exception>
        /// <returns></returns>
        private IHttpController GetHttpController(ActionContext actionContext)
        {
            try
            {
                var controllerType = actionContext.Action.DeclaringService;
                var controller = this.DependencyResolver.GetService(controllerType) as HttpController;
                controller.Middleware = this;
                return controller;
            }
            catch (Exception ex)
            {
                throw new ResolveException(actionContext.Action.DeclaringService, ex);
            }
        }
    }
}
