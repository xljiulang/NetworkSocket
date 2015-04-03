using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast.Internal
{
    /// <summary>
    /// FastTcp公共类
    /// </summary>
    internal static class FastTcpCommon
    {
        /// <summary>
        /// 获取服务类型的服务行为
        /// </summary>
        /// <param name="seviceType">服务类型</param>
        /// <returns></returns>
        public static List<FastAction> GetServiceFastActions(Type seviceType)
        {
            var methods = seviceType.GetMethods().Where(item => Attribute.IsDefined(item, typeof(ServiceAttribute)));
            return methods.Select(method => new FastAction(method)).ToList();
        }

        /// <summary>
        /// 设置服务行为返回的任务结果
        /// </summary>
        /// <param name="actionContext">上下文</param>
        public static void SetFastActionTaskResult(ActionContext actionContext)
        {
            var callBack = CallbackTable.Take(actionContext.Packet.HashCode);
            if (callBack != null)
            {
                var returnBytes = actionContext.Packet.GetBodyParameter().FirstOrDefault();
                callBack(actionContext.Packet.IsException, returnBytes);
            }
        }


        /// <summary>
        /// 设置服务行为返回的任务异常
        /// 如果无法失败，则返回异常上下文对象
        /// </summary>       
        /// <param name="requestContext">请求上下文</param>
        /// <param name="tcpServer">Tcp服务器</param>
        /// <returns></returns>
        public static ExceptionContext SetFastActionTaskException(this IFastTcpServer tcpServer, RequestContext requestContext)
        {
            return FastTcpCommon.SetFastActionTaskException(requestContext, tcpServer.Serializer);
        }

        /// <summary>
        /// 设置服务行为返回的任务异常
        /// 如果无法失败，则返回异常上下文对象
        /// </summary>       
        /// <param name="requestContext">请求上下文</param>
        /// <param name="tcpClient">Tcp客户端</param>
        /// <returns></returns>
        public static ExceptionContext SetFastActionTaskException(this FastTcpClientBase tcpClient, RequestContext requestContext)
        {
            return FastTcpCommon.SetFastActionTaskException(requestContext, tcpClient.Serializer);
        }

        /// <summary>
        /// 设置服务行为返回的任务异常
        /// 如果无法失败，则返回异常上下文对象
        /// </summary>       
        /// <param name="requestContext">请求上下文</param>
        /// <param name="serializer">序列化工具</param>
        /// <returns></returns>
        private static ExceptionContext SetFastActionTaskException(RequestContext requestContext, ISerializer serializer)
        {
            var bytes = requestContext.Packet.GetBodyParameter().FirstOrDefault();
            var exceptionCallBack = CallbackTable.Take(requestContext.Packet.HashCode);

            if (exceptionCallBack != null)
            {
                exceptionCallBack(true, bytes);
                return null;
            }
            else
            {
                var message = (string)serializer.Deserialize(bytes, typeof(string));
                var exception = new RemoteException(message);
                return new ExceptionContext(requestContext, exception);
            }
        }

        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="exceptionContext">上下文</param> 
        /// <param name="tcpServer">tcp服务器</param>
        public static void SetRemoteException(this IFastTcpServer tcpServer, ExceptionContext exceptionContext)
        {
            FastTcpCommon.SetRemoteException(exceptionContext, tcpServer.Serializer);
        }

        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="exceptionContext">上下文</param> 
        /// <param name="tcpServer">tcp客户端</param>
        public static void SetRemoteException(this FastTcpClientBase tcpClient, ExceptionContext exceptionContext)
        {
            FastTcpCommon.SetRemoteException(exceptionContext, tcpClient.Serializer);
        }

        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="exceptionContext">上下文</param> 
        /// <param name="tcpServer">Fast服务</param>
        public static void SetRemoteException(this IFastService fastService, ExceptionContext exceptionContext)
        {
            FastTcpCommon.SetRemoteException(exceptionContext, fastService.FastTcpServer.Serializer);
        }


        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="exceptionContext">上下文</param> 
        /// <param name="serializer">序列化工具</param>
        private static void SetRemoteException(ExceptionContext exceptionContext, ISerializer serializer)
        {
            exceptionContext.Packet.SetException(serializer, exceptionContext.Exception.Message);
            exceptionContext.Client.Send(exceptionContext.Packet);
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="serializer">序列化工具</param>
        /// <param name="commond">数据包的Action值</param>
        /// <param name="parameters">参数列表</param>
        /// <exception cref="RemoteException"></exception>
        public static void InvokeRemote(SocketAsync<FastPacket> client, ISerializer serializer, int commond, params object[] parameters)
        {
            var packet = new FastPacket(commond);
            packet.SetBodyBinary(serializer, parameters);
            client.Send(packet);
        }

        /// <summary>
        /// 将数据发送到远程端     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="serializer">序列化工具</param>
        /// <param name="commond">数据包的命令值</param>
        /// <param name="parameters"></param>
        /// <returns>参数列表</returns>
        public static Task<T> InvokeRemote<T>(SocketAsync<FastPacket> client, ISerializer serializer, int commond, params object[] parameters)
        {
            var taskSource = new TaskCompletionSource<T>();
            var packet = new FastPacket(commond);
            packet.SetBodyBinary(serializer, parameters);

            // 发送之前记录回参数
            Action<bool, byte[]> callBack = (isException, bytes) =>
            {
                if (isException == false)
                {
                    var result = (T)serializer.Deserialize(bytes, typeof(T));
                    taskSource.SetResult(result);
                }
                else
                {
                    var message = (string)serializer.Deserialize(bytes, typeof(string));
                    var exception = new RemoteException(message);
                    taskSource.TrySetException(exception);
                }
            };

            CallbackTable.Add(packet.HashCode, callBack);
            client.Send(packet);
            return taskSource.Task;
        }

        /// <summary>
        /// 生成服务行为的调用参数
        /// </summary>        
        /// <param name="context">上下文</param>       
        /// <param name="fastServer">服务</param>
        /// <returns></returns>
        public static object[] GetFastActionParameters(this IFastService fastServer, ActionContext context)
        {
            var items = context.Packet.GetBodyParameter();
            var parameters = new object[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                parameters[i] = fastServer.FastTcpServer.Serializer.Deserialize(items[i], context.Action.ParameterTypes[i]);
            }
            return parameters;
        }

        /// <summary>
        /// 生成服务行为的调用参数
        /// </summary>        
        /// <param name="context">上下文</param>       
        /// <param name="tcpClient">客户端</param>
        /// <returns></returns>
        public static object[] GetFastActionParameters(this FastTcpClientBase tcpClient, ActionContext context)
        {
            var items = context.Packet.GetBodyParameter();
            var parameters = new object[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                parameters[i] = tcpClient.Serializer.Deserialize(items[i], context.Action.ParameterTypes[i]);
            }
            return parameters;
        }

        /// <summary>
        /// 生成服务行为的调用参数
        /// </summary>        
        /// <param name="context">上下文</param>       
        /// <param name="serializer">序列化工具</param>
        /// <returns></returns>
        private static object[] GetFastActionParameters(ActionContext context, ISerializer serializer)
        {
            var items = context.Packet.GetBodyParameter();
            var parameters = new object[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                parameters[i] = serializer.Deserialize(items[i], context.Action.ParameterTypes[i]);
            }
            return parameters;
        }

        /// <summary>
        /// 检测服务行为是否有声明相同的Command
        /// </summary>
        /// <param name="actions">服务行为</param>
        public static void CheckActionsRepeatCommand(IEnumerable<FastAction> actions)
        {
            var group = actions.GroupBy(item => item.Command).FirstOrDefault(g => g.Count() > 1);
            if (group != null)
            {
                throw new Exception(string.Format("Command为{0}不允许被重复使用", group.Key));
            }
        }


        /// <summary>
        /// 检测服务行为的返回类型
        /// </summary>
        /// <param name="actions">服务行为</param>
        public static void CheckActionsTaskOrVoid(IEnumerable<FastAction> actions)
        {
            foreach (var action in actions)
            {
                FastTcpCommon.CheckActionTaskOrVoid(action);
            }
        }


        /// <summary>
        /// 检测服务行为的返回类型
        /// </summary>
        /// <param name="action">服务行为</param>
        private static void CheckActionTaskOrVoid(FastAction action)
        {
            if (action.Implement == Implements.Remote)
            {
                var isTask = action.ReturnType.IsGenericType && action.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
                if ((action.IsVoidReturn || isTask) == false)
                {
                    throw new Exception(string.Format("服务行为{0}的的返回类型必须是Task<T>类型", action.Name));
                }
            }
        }


        /// <summary>
        /// 在服务行为前 执行过滤器
        /// </summary>
        /// <param name="fastService">服务实例</param>
        /// <param name="actionFilters">服务行为过滤器</param>
        /// <param name="actionContext">上下文</param>   
        public static void ExecFiltersBeforeAction(this FastServiceBase fastService, IEnumerable<IFilter> actionFilters, ActionContext actionContext)
        {
            // OnAuthorization
            foreach (var globalFilter in GlobalFilters.AuthorizationFilters)
            {
                globalFilter.OnAuthorization(actionContext);
            }
            fastService.OnAuthorization(actionContext);
            foreach (var filter in actionFilters)
            {
                var authorizationFilter = filter as IAuthorizationFilter;
                if (authorizationFilter != null)
                {
                    authorizationFilter.OnAuthorization(actionContext);
                }
            }

            // OnExecuting
            foreach (var globalFilter in GlobalFilters.ActionFilters)
            {
                globalFilter.OnExecuting(actionContext);
            }
            fastService.OnExecuting(actionContext);
            foreach (var filter in actionFilters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuting(actionContext);
                }
            }
        }

        /// <summary>
        /// 在服务行为后执行过滤器
        /// </summary>
        /// <param name="fastService">服务实例</param>
        /// <param name="actionFilters">服务行为过滤器</param>
        /// <param name="actionContext">上下文</param>       
        public static void ExecFiltersAfterAction(this FastServiceBase fastService, IEnumerable<IFilter> actionFilters, ActionContext actionContext)
        {
            // 全局过滤器
            foreach (var globalFilter in GlobalFilters.ActionFilters)
            {
                globalFilter.OnExecuted(actionContext);
            }

            // 自身过滤器
            fastService.OnExecuted(actionContext);

            // 特性过滤器
            foreach (var filter in actionFilters)
            {
                var actionFilter = filter as IActionFilter;
                if (actionFilter != null)
                {
                    actionFilter.OnExecuted(actionContext);
                }
            }
        }

        /// <summary>
        /// 执行异常过滤器
        /// </summary>
        /// <param name="fastService">服务实例</param>
        /// <param name="actionFilters">服务行为过滤器</param>
        /// <param name="exceptionContext">上下文</param>       
        public static void ExecExceptionFilters(this FastServiceBase fastService, IEnumerable<IFilter> actionFilters, ExceptionContext exceptionContext)
        {
            foreach (var filter in GlobalFilters.ExceptionFilters)
            {
                if (exceptionContext.ExceptionHandled == false)
                {
                    filter.OnException(exceptionContext);
                }
            }

            if (exceptionContext.ExceptionHandled == false)
            {
                fastService.OnException(exceptionContext);
            }

            foreach (var filter in actionFilters)
            {
                var exceptionFilter = filter as IExceptionFilter;
                if (exceptionFilter != null && exceptionContext.ExceptionHandled == false)
                {
                    exceptionFilter.OnException(exceptionContext);
                }
            }
        }


        /// <summary>
        /// 执行异常过滤器
        /// </summary>
        /// <param name="tcpServer">服务实例</param>    
        /// <param name="exceptionContext">上下文</param>       
        public static void ExecExceptionFilters(this IFastTcpServer tcpServer, ExceptionContext exceptionContext)
        {
            foreach (var filter in GlobalFilters.ExceptionFilters)
            {
                if (exceptionContext.ExceptionHandled == false)
                {
                    filter.OnException(exceptionContext);
                }
            }
        }
    }
}
