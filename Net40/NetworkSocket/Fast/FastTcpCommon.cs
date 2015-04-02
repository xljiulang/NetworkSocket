using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
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
        public static List<FastAction> GetServiceActions(Type seviceType)
        {
            var methods = seviceType.GetMethods().Where(item => Attribute.IsDefined(item, typeof(ServiceAttribute)));
            return methods.Select(method => new FastAction(method)).ToList();
        }

        /// <summary>
        /// 抛出远程异常
        /// 如果无法抛出，则返回远程异常
        /// </summary>       
        /// <param name="exceptionPacket">异常包</param>
        /// <param name="serializer">序列化工具</param>
        /// <returns></returns>
        public static RemoteException ThrowRemoteException(FastPacket exceptionPacket, ISerializer serializer)
        {
            if (exceptionPacket.IsException == false)
            {
                return null;
            }

            var bytes = exceptionPacket.GetBodyParameter().FirstOrDefault();
            var exceptionCallBack = CallbackTable.Take(exceptionPacket.HashCode);

            if (exceptionCallBack != null)
            {
                exceptionCallBack(exceptionPacket.IsException, bytes);
                return null;
            }
            return serializer.Deserialize(bytes, typeof(RemoteException)) as RemoteException;
        }

        /// <summary>       
        /// 触发远程异常
        /// </summary>
        /// <param name="context">上下文</param> 
        /// <param name="serializer">序列化工具</param>
        public static void RaiseRemoteException(ExceptionContext context, ISerializer serializer)
        {
            var remoteException = new RemoteException(context.Packet.Command, context.Exception.ToString());
            context.Packet.SetException(serializer, remoteException);
            context.Client.Send(context.Packet);
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="serializer">序列化工具</param>
        /// <param name="cmd">数据包的Action值</param>
        /// <param name="parameters">参数列表</param>
        /// <exception cref="RemoteException"></exception>
        public static void InvokeRemote(SocketAsync<FastPacket> client, ISerializer serializer, int cmd, params object[] parameters)
        {
            var packet = new FastPacket(cmd);
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
        /// <param name="cmd">数据包的命令值</param>
        /// <param name="parameters"></param>
        /// <returns>参数列表</returns>
        public static Task<T> InvokeRemote<T>(SocketAsync<FastPacket> client, ISerializer serializer, int cmd, params object[] parameters)
        {
            var taskSource = new TaskCompletionSource<T>();
            var packet = new FastPacket(cmd);
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
                    var exception = serializer.Deserialize(bytes, typeof(RemoteException)) as RemoteException;
                    if (exception != null)
                    {
                        taskSource.TrySetException(exception);
                    }
                }
            };

            CallbackTable.Add(packet.HashCode, callBack);
            client.Send(packet);
            return taskSource.Task;
        }

        /// <summary>
        /// 生成客户端服务行为的调用参数
        /// </summary>        
        /// <param name="context">上下文</param>       
        /// <param name="serializer">序列化工具</param>
        /// <returns></returns>
        public static object[] GetActionParameters(ActionContext context, ISerializer serializer)
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
        /// 检测服务行为的声明和参数
        /// </summary>
        /// <param name="actions">服务行为</param>
        public static void CheckActionsContract(IEnumerable<FastAction> actions)
        {
            foreach (var action in actions)
            {
                FastTcpCommon.CheckActionContract(action);
            }
        }


        /// <summary>
        /// 检测服务行为的声明和参数
        /// </summary>
        /// <param name="action">服务行为</param>
        private static void CheckActionContract(FastAction action)
        {
            if (Enum.IsDefined(typeof(SpecialCommands), action.Command) && action.IsDefined(typeof(SpecialServiceAttribute), true) == false)
            {
                throw new Exception(string.Format("服务行为{0}的Command是不允许使用的SpecialCommand命令", action.Name));
            }

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
        /// <param name="service">服务实例</param>
        /// <param name="actionFilters">服务行为过滤器</param>
        /// <param name="actionContext">上下文</param>   
        public static void ExecFiltersBeforeAction(this FastServiceBase service, IEnumerable<IFilter> actionFilters, ActionContext actionContext)
        {
            // OnAuthorization
            foreach (var globalFilter in GlobalFilters.AuthorizationFilters)
            {
                globalFilter.OnAuthorization(actionContext);
            }
            service.OnAuthorization(actionContext);
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
            service.OnExecuting(actionContext);
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
        /// <param name="service">服务实例</param>
        /// <param name="actionFilters">服务行为过滤器</param>
        /// <param name="actionContext">上下文</param>       
        public static void ExecFiltersAfterAction(this FastServiceBase service, IEnumerable<IFilter> actionFilters, ActionContext actionContext)
        {
            // 全局过滤器
            foreach (var globalFilter in GlobalFilters.ActionFilters)
            {
                globalFilter.OnExecuted(actionContext);
            }

            // 自身过滤器
            service.OnExecuted(actionContext);

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
        /// <param name="service">服务实例</param>
        /// <param name="actionFilters">服务行为过滤器</param>
        /// <param name="exceptionContext">上下文</param>       
        public static void ExecExceptionFilters(this FastServiceBase service, IEnumerable<IFilter> actionFilters, ExceptionContext exceptionContext)
        {
            FastTcpCommon.RaiseRemoteException(exceptionContext, service.Serializer);

            foreach (var filter in GlobalFilters.ExceptionFilters)
            {
                if (exceptionContext.ExceptionHandled == false)
                {
                    filter.OnException(exceptionContext);
                }
            }

            if (exceptionContext.ExceptionHandled == false)
            {
                service.OnException(exceptionContext);
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
        /// <param name="server">服务实例</param>    
        /// <param name="exceptionContext">上下文</param>       
        public static void ExecExceptionFilters(this FastTcpServerBase server,  ExceptionContext exceptionContext)
        {
            FastTcpCommon.RaiseRemoteException(exceptionContext, server.Serializer);

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
