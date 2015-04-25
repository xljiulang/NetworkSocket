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
        /// 获取服务类型的Api行为
        /// </summary>
        /// <param name="seviceType">服务类型</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static IEnumerable<ApiAction> GetServiceApiActions(Type seviceType)
        {
            return seviceType
                .GetMethods()
                .Where(item => Attribute.IsDefined(item, typeof(ApiAttribute)))
                .Select(method => new ApiAction(method));
        }

        /// <summary>
        /// 设置Api行为返回的任务结果
        /// </summary>
        /// <param name="requestContext">上下文</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        public static void SetApiActionTaskResult(RequestContext requestContext, TaskSetActionTable taskSetActionTable)
        {
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.Id);
            if (taskSetAction != null)
            {
                var returnBytes = requestContext.Packet.Body;
                taskSetAction.SetAction(SetTypes.SetReturnReult, returnBytes);
            }
        }


        /// <summary>
        /// 设置Api行为返回的任务异常 
        /// 设置失败则返远程异常对象
        /// </summary>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        public static RemoteException SetApiActionTaskException(TaskSetActionTable taskSetActionTable, RequestContext requestContext)
        {
            var exceptionBytes = requestContext.Packet.Body;
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.Id);

            if (taskSetAction != null)
            {
                taskSetAction.SetAction(SetTypes.SetReturnException, exceptionBytes);
                return null;
            }

            var message = Encoding.UTF8.GetString(exceptionBytes);
            return new RemoteException(message);
        }

        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="session">会话对象</param>       
        /// <param name="exceptionContext">上下文</param>  
        /// <returns></returns>
        public static bool SetRemoteException(ISession session, ExceptionContext exceptionContext)
        {
            var packet = exceptionContext.Packet;
            packet.IsException = true;
            packet.Body = Encoding.UTF8.GetBytes(exceptionContext.Exception.Message);
            try
            {
                session.Send(packet.ToByteRange());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 调用远程端的Api     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="session">会话对象</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="serializer">序列化工具</param>   
        /// <param name="api">api</param>
        /// <param name="id">标识符</param>
        /// <param name="fromClient">是否为客户端封包</param>
        /// <param name="parameters">参数</param>      
        /// <exception cref="SocketException"></exception> 
        /// <exception cref="RemoteException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        public static Task<T> InvokeApi<T>(ISession session, TaskSetActionTable taskSetActionTable, ISerializer serializer, string api, long id, bool fromClient, params object[] parameters)
        {
            var taskSource = new TaskCompletionSource<T>();
            var packet = new FastPacket(api, id, fromClient);
            packet.SetBodyParameters(serializer, parameters);

            // 登记TaskSetAction           
            var setAction = FastTcpCommon.NewSetAction<T>(taskSource, serializer);
            var taskSetAction = new TaskSetAction(setAction);
            taskSetActionTable.Add(packet.Id, taskSetAction);
            session.Send(packet.ToByteRange());
            return taskSource.Task;
        }

        /// <summary>
        /// 创建新的SetAction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskSource">任务源</param>       
        /// <param name="serializer">序列化工具</param>      
        /// <returns></returns>
        private static Action<SetTypes, byte[]> NewSetAction<T>(TaskCompletionSource<T> taskSource, ISerializer serializer)
        {
            Action<SetTypes, byte[]> setAction = (setType, bytes) =>
            {
                if (setType == SetTypes.SetReturnReult)
                {
                    if (bytes == null || bytes.Length == 0)
                    {
                        taskSource.TrySetResult(default(T));
                        return;
                    }

                    try
                    {
                        var result = (T)serializer.Deserialize(bytes, typeof(T));
                        taskSource.TrySetResult(result);
                    }
                    catch (SerializerException ex)
                    {
                        taskSource.TrySetException(ex);
                    }
                    catch (Exception ex)
                    {
                        taskSource.TrySetException(new SerializerException(ex));
                    }
                }
                else if (setType == SetTypes.SetReturnException)
                {
                    var message = bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes);
                    var exception = new RemoteException(message);
                    taskSource.TrySetException(exception);
                }
                else if (setType == SetTypes.SetTimeoutException)
                {
                    var exception = new TimeoutException();
                    taskSource.TrySetException(exception);
                }
                else if (setType == SetTypes.SetShutdownException)
                {
                    var exception = new SocketException(SocketError.Shutdown.GetHashCode());
                    taskSource.TrySetException(exception);
                }
            };
            return setAction;
        }


        /// <summary>
        /// 生成Api行为的调用参数
        /// </summary>        
        /// <param name="serializer">序列化工具</param>
        /// <param name="context">上下文</param> 
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        public static object[] GetApiActionParameters(ISerializer serializer, ActionContext context)
        {
            var bodyParameters = context.Packet.GetBodyParameters();
            var parameters = new object[bodyParameters.Count];

            for (var i = 0; i < bodyParameters.Count; i++)
            {
                var parameterBytes = bodyParameters[i];
                var parameterType = context.Action.ParameterTypes[i];

                if (parameterBytes == null || parameterBytes.Length == 0)
                {
                    parameters[i] = Activator.CreateInstance(parameterType);
                }
                else
                {
                    parameters[i] = serializer.Deserialize(parameterBytes, parameterType);
                }
            }
            return parameters;
        }
    }
}
