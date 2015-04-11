using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        public static List<FastAction> GetServiceFastActions(Type seviceType)
        {
            var methods = seviceType.GetMethods().Where(item => Attribute.IsDefined(item, typeof(ServiceAttribute)));
            return methods.Select(method => new FastAction(method)).ToList();
        }

        /// <summary>
        /// 设置服务行为返回的任务结果
        /// </summary>
        /// <param name="requestContext">上下文</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        public static void SetFastActionTaskResult(RequestContext requestContext, TaskSetActionTable taskSetActionTable)
        {
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.HashCode);
            if (taskSetAction != null)
            {
                var returnBytes = requestContext.Packet.GetBodyParameter().FirstOrDefault();
                taskSetAction.SetAction(SetTypes.SetReult, returnBytes);
            }
        }


        /// <summary>
        /// 设置服务行为返回的任务异常
        /// 如果无法失败，则返回异常上下文对象
        /// </summary>  
        /// <param name="serializer">序列化工具</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        public static ExceptionContext SetFastActionTaskException(ISerializer serializer, TaskSetActionTable taskSetActionTable, RequestContext requestContext)
        {
            var exceptionBytes = requestContext.Packet.GetBodyParameter().FirstOrDefault();
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.HashCode);

            if (taskSetAction != null)
            {
                taskSetAction.SetAction(SetTypes.SetException, exceptionBytes);
                return null;
            }
            else
            {
                var message = (string)serializer.Deserialize(exceptionBytes, typeof(string));
                var exception = new RemoteException(message);
                return new ExceptionContext(requestContext, exception);
            }
        }

        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="serializer">序列化工具</param>
        /// <param name="exceptionContext">上下文</param>        
        public static void SetRemoteException(ISerializer serializer, ExceptionContext exceptionContext)
        {
            exceptionContext.Packet.SetException(serializer, exceptionContext.Exception.Message);
            exceptionContext.Client.Send(exceptionContext.Packet);
        }

        /// <summary>
        /// 将数据发送到远程端     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="serializer">序列化工具</param>   
        /// <param name="command">数据包的命令值</param>
        /// <param name="hashCode">哈希码</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SocketException"></exception> 
        /// <exception cref="RemoteException"></exception>
        /// <returns></returns>
        public static Task<T> InvokeRemote<T>(SocketAsync<FastPacket> client, TaskSetActionTable taskSetActionTable, ISerializer serializer, int command, long hashCode, params object[] parameters)
        {
            var taskSource = new TaskCompletionSource<T>();
            var packet = new FastPacket(command, hashCode);
            packet.SetBodyBinary(serializer, parameters);

            // 登记TaskSetAction           
            Action<SetTypes, byte[]> setAction = (setType, bytes) =>
            {
                if (setType == SetTypes.SetReult)
                {
                    var result = (T)serializer.Deserialize(bytes, typeof(T));
                    taskSource.TrySetResult(result);
                }
                else if (setType == SetTypes.SetException)
                {
                    var message = (string)serializer.Deserialize(bytes, typeof(string));
                    var exception = new RemoteException(message);
                    taskSource.TrySetException(exception);
                }
                else if (setType == SetTypes.SetTimeout)
                {
                    var exception = new TimeoutException();
                    taskSource.TrySetException(exception);
                }
            };
            var taskSetAction = new TaskSetAction(setAction);
            taskSetActionTable.Add(packet.HashCode, taskSetAction);

            client.Send(packet);
            return taskSource.Task;
        }


        /// <summary>
        /// 生成服务行为的调用参数
        /// </summary>        
        /// <param name="serializer">序列化工具</param>
        /// <param name="context">上下文</param> 
        /// <returns></returns>
        public static object[] GetFastActionParameters(ISerializer serializer, ActionContext context)
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
    }
}
